using System;
using System.Drawing;
using System.Windows.Forms;

namespace SayMore.UI.ProjectChoosingAndCreating.NewProjectDialog
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Dialog for allowing user to enter the name of a new project.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public partial class NewProjectDlg : Form
	{
		private readonly NewProjectDlgViewModel _viewModel;

		/// ------------------------------------------------------------------------------------
		public NewProjectDlg()
		{
			InitializeComponent();
			_buttonOK.Enabled = false;
			_newProjectPathLabel.Text = string.Empty;
			_newProjectPathLabel.Font = new Font(SystemFonts.MessageBoxFont.FontFamily, 8f, GraphicsUnit.Point);
		}

		/// ------------------------------------------------------------------------------------
		public NewProjectDlg(NewProjectDlgViewModel viewModel) : this()
		{
			_viewModel = viewModel;
		}

		/// ------------------------------------------------------------------------------------
		protected override void OnActivated(EventArgs e)
		{
			base.OnActivated(e);

			// Do this to update the message label if we've just come back
			// from the localization dialog box.
			txtName_TextChanged(null, null);
		}

		/// ------------------------------------------------------------------------------------
		protected void txtName_TextChanged(object sender, EventArgs e)
		{
			_buttonOK.Enabled = _viewModel.IsNewProjectNameValid(
				_nameTextBox.Text.Trim(), _newProjectPathLabel);
		}
	}
}