using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Windows.Forms;
using Palaso.Code;
using SayMore.Properties;
using SilUtils;

namespace SayMore.ClearShare
{
	/// ----------------------------------------------------------------------------------------
	public partial class ContributorsListControl : UserControl
	{
		public delegate string ValidatingContributorHandler(ContributorsListControl sender,
			Contribution contribution, CancelEventArgs e);

		public event ValidatingContributorHandler ValidatingContributor;
		public event EventHandler ContributorDeleted;

		private Contribution _preValidatedContribution;
		private readonly ContributorsListControlViewModel _model;
		private int _indexOfIncompleteRowToDelete = -1;

		/// ------------------------------------------------------------------------------------
		public ContributorsListControl()
		{
			InitializeComponent();
		}

		/// ------------------------------------------------------------------------------------
		public ContributorsListControl(ContributorsListControlViewModel model) : this()
		{
			_model = model;
			_model.NewContributionListAvailable += HandleNewContributionListAvailable;
			Initialize();
		}

		/// ------------------------------------------------------------------------------------
		private void Initialize()
		{
			_grid.Font = SystemFonts.IconTitleFont;

			DataGridViewColumn col = SilGrid.CreateTextBoxColumn("name", "Name");
			col.Width = 150;
			_grid.Columns.Add(col);

			col = SilGrid.CreateDropDownListComboBoxColumn("role",
				_model.OlacRoles.Select(r => r.ToString()));
			col.HeaderText = "Role";
			col.Width = 120;
			_grid.Columns.Add(col);

			_grid.Columns.Add(SilGrid.CreateCalendarControlColumn("date", "Date"));

			col = SilGrid.CreateTextBoxColumn("notes", "Notes");
			col.Width = 200;
			_grid.Columns.Add(col);

			_grid.AllowUserToAddRows = true;
			_grid.AllowUserToDeleteRows = true;
			_grid.EditingControlShowing += HandleEditingControlShowing;
			_grid.CurrentRowChanged += HandleGridCurrentRowChanged;
			_grid.RowValidating += HandleGridRowValidating;
			_grid.MouseClick += HandleGridMouseClick;
			_grid.Enter += HandleGridEnter;
			_grid.Leave += HandleGridLeave;

			if (Settings.Default.ContributorsGrid != null)
				Settings.Default.ContributorsGrid.InitializeGrid(_grid);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a value indicating whether or not the extender is currently in design mode.
		/// I have had some problems with the base class' DesignMode property being true
		/// when in design mode. I'm not sure why, but adding a couple more checks fixes the
		/// problem.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private new bool DesignMode
		{
			get
			{
				return (base.DesignMode || GetService(typeof(IDesignerHost)) != null) ||
					(LicenseManager.UsageMode == LicenseUsageMode.Designtime);
			}
		}

		/// ------------------------------------------------------------------------------------
		protected override void OnHandleDestroyed(EventArgs e)
		{
			if (!DesignMode)
				Settings.Default.ContributorsGrid = GridSettings.Create(_grid);

			base.OnHandleDestroyed(e);
		}

		/// ------------------------------------------------------------------------------------
		void HandleNewContributionListAvailable(object sender, EventArgs e)
		{
			Guard.AgainstNull(_model.Contributions, "Contributions");

			// Add one for the new contributor row.
			_grid.RowCount = _model.Contributions.Count() + 1;
			_grid.CurrentCell = _grid[0, 0];
		}

		/// ------------------------------------------------------------------------------------
		private void UpdateDisplay()
		{
			_toolTip.SetToolTip(_buttonDelete, null);

			if (!_model.GetCanDeleteContribution(_grid.CurrentCellAddress.Y))
				_buttonDelete.Enabled = false;
			else
			{
				_buttonDelete.Enabled = true;
				var name = _model.GetContributionValue(_grid.CurrentCellAddress.Y, "name") as string;
				if (!string.IsNullOrEmpty(name))
				{
					var tooltip = string.Format("Delete contributor '{0}'", name);
					_toolTip.SetToolTip(_buttonDelete, tooltip);
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		void HandleGridCurrentRowChanged(object sender, EventArgs e)
		{
			_preValidatedContribution = _model.GetContributionCopy(_grid.CurrentCellAddress.Y);
			UpdateDisplay();
		}

		/// ------------------------------------------------------------------------------------
		void HandleGridMouseClick(object sender, MouseEventArgs e)
		{
			var hi = _grid.HitTest(e.X, e.Y);

			// Make the first cell current when the user clicks a row heading.
			if (e.Button == MouseButtons.Left && hi.ColumnIndex == -1 &&
				hi.RowIndex >= 0 && hi.RowIndex < _grid.RowCount)
			{
				_grid.CurrentCell = _grid[0, hi.RowIndex];
			}
		}

		/// ------------------------------------------------------------------------------------
		void HandleGridEnter(object sender, EventArgs e)
		{
			_grid.SelectionMode = DataGridViewSelectionMode.CellSelect;
		}

		/// ------------------------------------------------------------------------------------
		void HandleGridLeave(object sender, EventArgs e)
		{
			_grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

			if (_grid.CurrentRow != null)
				_grid.CurrentRow.Selected = true;
		}

		/// ------------------------------------------------------------------------------------
		private void HandleGridRowValidating(object sender, DataGridViewCellCancelEventArgs e)
		{
			if (ValidatingContributor == null || e.RowIndex == _grid.RowCount - 1)
				return;

			var contribution = _model.Contributions.ElementAt(e.RowIndex);

			// Don't bother doing anything if the contribution didn't change.
			if (contribution.AreContentsEqual(_preValidatedContribution))
				return;

			// If the name is missing, then get rid of the current row.
			// See more explanation of the process in RemoveIncompleteRow.
			if (contribution.ContributorName == null || contribution.ContributorName.Trim().Length == 0)
			{
				_grid.EndEdit();
				_indexOfIncompleteRowToDelete = e.RowIndex;
				Application.Idle += RemoveIncompleteRow;
				return;
			}

			var args = new CancelEventArgs(e.Cancel);
			var errorMsg = ValidatingContributor(this, contribution, args);
			e.Cancel = args.Cancel;

			if (!string.IsNullOrEmpty(errorMsg))
				Palaso.Reporting.ProblemNotificationDialog.Show(errorMsg);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// The best time to remove an imcomplete row is when it's determined that the row is
		/// incomplete (i.e. in the row validating event). However, removing the row involves
		/// changing the row count. The problem is, the grid throws an exception if you try
		/// to change the row count in any of the row handling events (e.g. RowValidating,
		/// RowValidated, RowLeave, RowEnter, etc.). The only way I know of to reliably get
		/// rid of the row when the user would expect it, is when the app. goes idle right
		/// after the row is validated and it's determined the row is incomplete. Argh!
		/// </summary>
		/// ------------------------------------------------------------------------------------
		void RemoveIncompleteRow(object sender, EventArgs e)
		{
			if (_indexOfIncompleteRowToDelete >= 0)
				DeleteRow(_indexOfIncompleteRowToDelete);

			Application.Idle -= RemoveIncompleteRow;
		}

		/// ------------------------------------------------------------------------------------
		private void HandleCellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
		{
			var value = _model.GetContributionValue(e.RowIndex, _grid.Columns[e.ColumnIndex].Name);
			if (value != null)
				e.Value = value;
		}

		/// ------------------------------------------------------------------------------------
		private void HandleCellValuePushed(object sender, DataGridViewCellValueEventArgs e)
		{
			_model.SetContributionValue(e.RowIndex, _grid.Columns[e.ColumnIndex].Name, e.Value);
		}

		/// ------------------------------------------------------------------------------------
		protected void HandleEditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
		{
			if (_grid.CurrentCellAddress.X != 0)
				return;

			var txtBox = e.Control as TextBox;
			txtBox.KeyPress += HandleCellEditBoxKeyPress;
			txtBox.HandleDestroyed += HandleCellEditBoxHandleDestroyed;
			txtBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
			txtBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
			txtBox.AutoCompleteCustomSource = _model.GetAutoCompleteNames();
		}

		/// ------------------------------------------------------------------------------------
		private static void HandleCellEditBoxKeyPress(object sender, KeyPressEventArgs e)
		{
			// Prevent characters that are invalid as xml tags. There's probably more,
			// but this will do for now.
			if ("<>{}()[]/'\"\\.,;:?|!@#$%^&*=+`~".IndexOf(e.KeyChar) >= 0)
			{
				e.KeyChar = (char)0;
				e.Handled = true;
				SystemSounds.Beep.Play();
			}
		}

		/// ------------------------------------------------------------------------------------
		private static void HandleCellEditBoxHandleDestroyed(object sender, EventArgs e)
		{
			((TextBox)sender).KeyPress -= HandleCellEditBoxKeyPress;
			((TextBox)sender).HandleDestroyed -= HandleCellEditBoxHandleDestroyed;
		}

		/// ------------------------------------------------------------------------------------
		private void HandleDeleteButtonClicked(object sender, EventArgs e)
		{
			DeleteRow(_grid.CurrentCellAddress.Y);
		}

		/// ------------------------------------------------------------------------------------
		private void DeleteRow(int rowIndex)
		{
			if (!_model.DeleteContribution(rowIndex))
				return;

			_grid.CurrentRowChanged -= HandleGridCurrentRowChanged;
			_grid.RowCount--;
			_grid.CurrentRowChanged += HandleGridCurrentRowChanged;

			if (rowIndex >= _grid.RowCount && rowIndex > 0)
				rowIndex--;

			_grid.CurrentCell = _grid[_grid.CurrentCellAddress.X, rowIndex];
			UpdateDisplay();

			if (ContributorDeleted != null)
				ContributorDeleted(this, EventArgs.Empty);
		}
	}
}