
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using L10NSharp;
using Palaso.UI.WindowsForms.Widgets.BetterGrid;
using SayMore.Media.FFmpeg;
using SayMore.Model.Files;
using SayMore.Properties;
using SIL.Archiving.IMDI.Schema;

namespace SayMore.UI.ComponentEditors
{
	public partial class PersonContributionEditor : EditorBase
	{
		public delegate PersonContributionEditor Factory(ComponentFile file, string imageKey);

		private BetterGrid _grid;
		private string _personId;

		public PersonContributionEditor(ComponentFile file, string imageKey)
			: base(file, null, imageKey)
		{
			InitializeComponent();
			Name = "PersonRoles";
			RememberPersonId(file);

			InitializeControls();

			// do this to set the Associated Sessions the first time because the project might not be loaded yet
			GetDataInBackground();
		}

		private void GetDataInBackground()
		{
			using (BackgroundWorker backgroundWorker = new BackgroundWorker())
			{
				backgroundWorker.DoWork += backgroundWorker_DoWork;
				backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
				backgroundWorker.RunWorkerAsync();
			}
		}

		void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			LoadAssociatedSessionsAndFiles();
		}

		void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			var count = 0;
			while ((Program.CurrentProject == null) && (count < 50))
			{
				Thread.Sleep(100);
				count++;
			}
		}

		private void InitializeControls()
		{
			_grid = new BetterGrid
			{
				Dock = DockStyle.Top,
				RowHeadersVisible = false,
				AllowUserToOrderColumns = false,
				AllowUserToResizeRows = false,
				AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells
			};
			_grid.Columns.Add(new DataGridViewTextBoxColumn { SortMode = DataGridViewColumnSortMode.NotSortable, ReadOnly = true, HeaderText = LocalizationManager.GetString("PeopleView.ContributionEditor.NameColumnTitle", "Name") });
			_grid.Columns.Add(new DataGridViewTextBoxColumn { SortMode = DataGridViewColumnSortMode.NotSortable, ReadOnly = true, HeaderText = LocalizationManager.GetString("PeopleView.ContributionEditor.RoleColumnTitle", "Role") });
			_grid.Columns.Add(new DataGridViewTextBoxColumn { SortMode = DataGridViewColumnSortMode.NotSortable, ReadOnly = true, HeaderText = LocalizationManager.GetString("PeopleView.ContributionEditor.DateColumnTitle", "Date") });
			_grid.Columns.Add(new DataGridViewTextBoxColumn { SortMode = DataGridViewColumnSortMode.NotSortable, ReadOnly = true, HeaderText = LocalizationManager.GetString("PeopleView.ContributionEditor.CommentColumnTitle", "Comments") });
			Controls.Add(_grid);
		}

		private void LoadAssociatedSessionsAndFiles()
		{
			var project = Program.CurrentProject;
			var boldFont = new Font(_grid.Font.FontFamily, _grid.Font.Size, FontStyle.Bold);

			if (project == null) return;

			_grid.Rows.Clear();

			foreach (var session in project.GetAllSessions())
			{
				var person = session.GetAllPersonsInSession().FirstOrDefault(p => p.Id == _personId);

				if (person != null)
				{
					// add Participant row for this session
					var sessionDescription = session.Id + " " + session.Title;
					//if (sessionDescription.Length > 30) sessionDescription = sessionDescription.Substring(0, 27) + "...";
					var sessionRole = LocalizationManager.GetString("PeopleView.ContributionEditor.RoleParticipant", "Participant");
					var sessionDate = session.MetaDataFile.GetStringValue("date", string.Empty);
					if (!string.IsNullOrEmpty(sessionDate)) sessionDate = DateTime.Parse(sessionDate).ToShortDateString();

					var rowid = _grid.AddRow(new object[] { sessionDescription, sessionRole, sessionDate, string.Empty });
					_grid.Rows[rowid].Cells[0].Style.Font = boldFont;
				}

				// get the files for this session
				var files = Directory.GetFiles(session.FolderPath, "*" + Settings.Default.MetadataFileExtension);
				var searchFor = "<name>" + _personId + "</name>";
				foreach (var file in files)
				{
					var fileContents = File.ReadAllText(file);

					// look for this person
					var pos = fileContents.IndexOf(searchFor, StringComparison.InvariantCultureIgnoreCase);
					while (pos > 0)
					{
						// remove everything before this person
						fileContents = fileContents.Substring(pos + searchFor.Length);

						// get the end of this contributor
						var testString = fileContents.Substring(0, fileContents.IndexOf("</contributor>", StringComparison.InvariantCultureIgnoreCase));

						var role = GetValueFromXmlString(testString, "role");
						var date = GetValueFromXmlString(testString, "date");
						if (!string.IsNullOrEmpty(date))
							date = DateTime.Parse(date).ToShortDateString();
						var note = GetValueFromXmlString(testString, "notes");
						var fname = Path.GetFileName(file.Substring(0, file.Length - Settings.Default.MetadataFileExtension.Length));
						_grid.AddRow(new object[] { Path.GetFileName(fname), role, date, note });

						// look again
						pos = fileContents.IndexOf(searchFor, StringComparison.InvariantCultureIgnoreCase);
					}
				}
			}
		}

		private string GetValueFromXmlString(string xmlString, string valueName)
		{
			var pattern = string.Format("<{0}>(.*)</{0}>", valueName);
			var match = Regex.Match(xmlString, pattern);

			return match.Success ? match.Groups[1].Value : string.Empty;
		}

		protected override void HandleStringsLocalized()
		{
			TabText = LocalizationManager.GetString("PeopleView.ContributionEditor.TabText", "Contributions");
			base.HandleStringsLocalized();
		}

		public override void SetComponentFile(ComponentFile file)
		{
			RememberPersonId(file);
			GetDataInBackground();
		}

		private void RememberPersonId(ComponentFile file)
		{
			_personId = file.FileName.Substring(0, file.FileName.Length - Settings.Default.PersonFileExtension.Length);
		}
	}
}
