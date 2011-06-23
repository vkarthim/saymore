using System;
using System.Drawing;
using System.Windows.Forms;
using Localization;

namespace SayMore.Transcription.UI
{
	public partial class CreateAnnotationFileDlg : Form
	{
		public string FileName { get; private set; }

		/// ------------------------------------------------------------------------------------
		public CreateAnnotationFileDlg()
		{
			InitializeComponent();

			_labelOverview.Font = SystemFonts.IconTitleFont;
			_labelAnnoatationType1.Font = SystemFonts.IconTitleFont;
			_labelAnnoatationType2.Font = SystemFonts.IconTitleFont;
			_labelAudacityOverview.Font = SystemFonts.IconTitleFont;
			_labelElanOverview.Font = SystemFonts.IconTitleFont;
		}

		/// ------------------------------------------------------------------------------------
		private void HandleLoadAudacityLabelFileClick(object sender, EventArgs e)
		{
			var caption = LocalizationManager.LocalizeString(
				"CreateAnnotationFileDlg.LoadAudacityLabelFileDlgCaption", "Select Audacity Label File");

			if (ShowOpenFileDialog(caption, "Audacity Label File (*.txt)|*.txt"))
				Close();
		}

		/// ------------------------------------------------------------------------------------
		private void HandleLoadSegmentFileClick(object sender, EventArgs e)
		{
			var caption = LocalizationManager.LocalizeString(
				"CreateAnnotationFileDlg.LoadSegmentFileDlgCaption", "Select Segment File");

			if (ShowOpenFileDialog(caption, "ELAN File (*.eaf)|*.eaf"))
				Close();
		}

		/// ------------------------------------------------------------------------------------
		private bool ShowOpenFileDialog(string caption, string filter)
		{
			using (var dlg = new OpenFileDialog())
			{
				dlg.Title = caption;
				dlg.CheckFileExists = true;
				dlg.CheckPathExists = true;
				dlg.Multiselect = false;
				dlg.Filter = filter + "|All Files (*.*)|*.*";

				if (dlg.ShowDialog() != DialogResult.OK)
					return false;

				FileName = dlg.FileName;
				DialogResult = DialogResult.OK;
				return true;
			}
		}
	}
}
