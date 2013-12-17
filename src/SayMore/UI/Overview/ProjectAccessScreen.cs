
using System.Linq;
using System.Runtime.Remoting;
using System.Windows.Forms;
using SIL.Archiving.Generic.AccessProtocol;

namespace SayMore.UI.Overview
{
	public partial class ProjectAccessScreen : UserControl
	{
		private bool _isLoaded;

		public ProjectAccessScreen()
		{
			InitializeComponent();

			// access protocol list
			var protocols = AccessProtocols.LoadStandardAndCustom();
			protocols.Insert(0, new ArchiveAccessProtocol { ProtocolName = "None" });
			_projectAccess.DataSource = protocols;

			SizeProtocolsComboBox(_projectAccess);
		}

		private void SizeProtocolsComboBox(ComboBox comboBox)
		{
			var maxWidth = 0;
			foreach (var item in comboBox.Items)
			{
				var itmWidth = TextRenderer.MeasureText(item.ToString(), comboBox.Font).Width;
				if (itmWidth > maxWidth)
					maxWidth = itmWidth;
			}

			comboBox.Width = maxWidth + 30;
		}

		private void _projectAccess_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (_projectAccess.SelectedItem == null) return;

			if (_isLoaded) SetForCustom();

			var item = (ArchiveAccessProtocol)_projectAccess.SelectedItem;

			// was the last item (Custom) selected?
			if (_projectAccess.SelectedIndex == _projectAccess.Items.Count - 1)
			{
				_customAccessChoices.Text = item.ChoicesToCsv();
			}
			else
			{
				_labelDescription.Text = item.DocumentationFile;
			}

		}

		private void SetForCustom()
		{
			var isCustom = _projectAccess.SelectedIndex == _projectAccess.Items.Count - 1;
			_labelCustomAccess.Visible = isCustom;
			_labelCustomInstructions.Visible = isCustom;
			_customAccessChoices.Visible = isCustom;
			_labelDescription.Visible = !isCustom;
		}

		private void ProjectAccessScreen_Leave(object sender, System.EventArgs e)
		{
			// check for changes
			var changed = false;
			var project = Program.CurrentProject;

			if (_projectAccess.Text != project.AccessProtocol)
				changed = true;

			// check if custom access choices changed
			ArchiveAccessProtocol custom = (ArchiveAccessProtocol) _projectAccess.Items[_projectAccess.Items.Count - 1];
			if (_customAccessChoices.Text != custom.ChoicesToCsv())
			{
				var customs = AccessProtocols.LoadCustom();
				var firstCustom = customs.First();
				firstCustom.SetChoicesFromCsv(_customAccessChoices.Text);
				_customAccessChoices.Text = firstCustom.ChoicesToCsv();
				AccessProtocols.SaveCustom(customs);
				changed = true;
			}

			if (!changed) return;

			// save changes
			project.AccessProtocol = _projectAccess.Text;
			project.Save();
		}

		private void ProjectAccessScreen_Load(object sender, System.EventArgs e)
		{
			// show values from project file
			var project = Program.CurrentProject;

			foreach (var item in _projectAccess.Items.Cast<object>().Where(i => i.ToString() == project.AccessProtocol))
				_projectAccess.SelectedItem = item;

			_isLoaded = true;

			SetForCustom();
		}
	}
}
