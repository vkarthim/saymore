using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using L10NSharp;
using Palaso.Reporting;
using Palaso.UI.WindowsForms.Widgets.BetterGrid;
using SayMore.Model;
using SayMore.Model.Files;
using SayMore.Properties;
using Palaso.Extensions;

namespace SayMore.UI.ElementListScreen
{
	public class SessionsGrid : ElementGrid
	{
		public delegate SessionsGrid Factory();  //autofac uses this

		private readonly StagesDataProvider _stagesDataProvider;
		private readonly StagesControlToolTip _tooltip;

		/// ------------------------------------------------------------------------------------
		public SessionsGrid(StagesDataProvider stagesDataProvider, StagesControlToolTip toolTip)
		{
			_stagesDataProvider = stagesDataProvider;
			_tooltip = toolTip;
		}

		/// ------------------------------------------------------------------------------------
		public override GridSettings GridSettings
		{
			get { return Settings.Default.SessionsListGrid; }
			set { Settings.Default.SessionsListGrid = value; }
		}

		/// ------------------------------------------------------------------------------------
		protected override object GetValueForField(ProjectElement element, string fieldName)
		{
			if (fieldName == SessionFileType.kStatusFieldName)
			{
				var value = base.GetValueForField(element, fieldName);
				return Resources.ResourceManager.GetObject("Status" + Session.GetStatusAsEnumParsableString((string)value));
			}

			if (fieldName == SessionFileType.kStagesFieldName)
				return _stagesDataProvider.CreateImageForComponentStage(element.GetCompletedStages());

			if (fieldName == SessionFileType.kDateFieldName)
			{
				var date = base.GetValueForField(element, fieldName);
				return date;
			}

			return base.GetValueForField(element, fieldName);
		}

		/// ------------------------------------------------------------------------------------
		protected override object GetSortValueForField(ProjectElement element, string fieldName)
		{
			if (fieldName == SessionFileType.kStatusFieldName)
			{
				var statusString = base.GetValueForField(element, fieldName) as string;
				var status = (Session.Status)Enum.Parse(typeof(Session.Status), Session.GetStatusAsEnumParsableString(statusString));
				return (int)status;
			}

			if (fieldName == SessionFileType.kStagesFieldName)
				return _stagesDataProvider.GetCompletedRolesKey(element.GetCompletedStages());

			if (fieldName == SessionFileType.kDateFieldName)
			{
				var dateString = base.GetValueForField(element, fieldName) as string;

				try
				{
					//we parse it and then generate it because we're trying to migrate old, locale-specific dates to ISO8601 dates
					return DateTimeExtensions.ParseDateTimePermissivelyWithException(dateString).ToISO8601DateOnlyString();
				}
				catch (Exception e)
				{
#if DEBUG
					ErrorReport.NotifyUserOfProblem(e, "only seeing because your'e in DEBUG mode");
#endif
					return DateTime.MinValue.ToISO8601DateOnlyString();
				}
			}

			return base.GetSortValueForField(element, fieldName) as string;
		}

		/// ------------------------------------------------------------------------------------
		public override IEnumerable<ToolStripMenuItem> GetMenuCommands()
		{
			// RAMP Archive
			var menu = new ToolStripMenuItem(string.Empty, Resources.RampIcon,
				(s, e) => {
					var session = (Session)GetCurrentElement();
					if (session != null)
						session.ArchiveUsingRAMP();
				});

			menu.Text = LocalizationManager.GetString("SessionsView.SessionsList.RampArchiveMenuText",
				"Archive with RAMP (SIL)...", null, menu);

			// Since this item isn't going to be added to an actual menu yet, we can't hook up the
			// code to enable/disable it yet. When it is added to a menu, if that menu is a drop-down
			// (which it will be), then we set up the handler to disable it if there is not a current
			// session.
			menu.OwnerChanged += (s, e) =>
			{
				if (menu.Owner != null && menu.Owner.IsDropDown)
					((ToolStripDropDown)menu.Owner).Opened += (s1, e1) => menu.Enabled = GetCurrentElement() is Session;
			};

			yield return menu;

			// IMDI Archive
			menu = new ToolStripMenuItem(string.Empty, null,
				(s, e) => {
					var session = (Session)GetCurrentElement();
					if (session != null)
						session.ArchiveUsingIMDI();
				});

			menu.Text = LocalizationManager.GetString("SessionsView.SessionsList.IMDIArchiveMenuText",
				"Archive using IMDI...", null, menu);

			// Since this item isn't going to be added to an actual menu yet, we can't hook up the
			// code to enable/disable it yet. When it is added to a menu, if that menu is a drop-down
			// (which it will be), then we set up the handler to disable it if there is not a current
			// session.
			menu.OwnerChanged += (s, e) =>
			{
				if (menu.Owner != null && menu.Owner.IsDropDown)
					((ToolStripDropDown)menu.Owner).Opened += (s1, e1) => menu.Enabled = GetCurrentElement() is Session;
			};

			yield return menu;

			if (DeleteAction != null)
			{
				menu = new ToolStripMenuItem(string.Empty, null, (s, e) => DeleteAction());
				menu.Text = LocalizationManager.GetString("MainWindow.DeleteSessionMenuText", "&Delete Session...", null, menu);
				yield return menu;
			}
		}

		/// ------------------------------------------------------------------------------------
		protected override void OnCellMouseEnter(DataGridViewCellEventArgs e)
		{
			base.OnCellMouseEnter(e);

			if (e.RowIndex >= 0 && e.ColumnIndex >= 0 &&
				Columns[e.ColumnIndex].DataPropertyName == SessionFileType.kStagesFieldName)
			{
				var element = _items.ElementAt(e.RowIndex);
				var pt = MousePosition;
				pt.Offset(5, 5);
				_tooltip.Show(pt, element.GetCompletedStages());
			}
		}

		/// ------------------------------------------------------------------------------------
		protected override void OnCellMouseLeave(DataGridViewCellEventArgs e)
		{
			base.OnCellMouseLeave(e);

			if (Columns[e.ColumnIndex].DataPropertyName == SessionFileType.kStagesFieldName)
				_tooltip.Hide();
		}

		/// ------------------------------------------------------------------------------------
		protected override void OnCellToolTipTextNeeded(DataGridViewCellToolTipTextNeededEventArgs e)
		{
			if (e.RowIndex >= 0 && e.ColumnIndex >= 0 &&
				Columns[e.ColumnIndex].DataPropertyName == SessionFileType.kStatusFieldName)
			{
				var value = base.GetValueForField(_items.ElementAt(e.RowIndex), SessionFileType.kStatusFieldName);
				var statusText = Session.GetLocalizedStatus(value as string);

				e.ToolTipText = string.Format(
					LocalizationManager.GetString("SessionsView.SessionStatus.TooltipFormatText", "Status: {0}"), statusText);
			}

			base.OnCellToolTipTextNeeded(e);
		}
	}
}
