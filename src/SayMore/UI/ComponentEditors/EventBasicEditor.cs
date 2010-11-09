using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SayMore.Model;
using SayMore.Model.Fields;
using SayMore.Model.Files;
using SayMore.Model.Files.DataGathering;
using SayMore.UI.LowLevelControls;

namespace SayMore.UI.ComponentEditors
{
	/// ----------------------------------------------------------------------------------------
	public partial class EventBasicEditor : EditorBase
	{
		public delegate EventBasicEditor Factory(ComponentFile file, string tabText, string imageKey);

		private FieldsValuesGrid _gridCustomFields;
		private FieldsValuesGridViewModel _gridViewModel;
		private PersonInformant _personInformant;

		/// ------------------------------------------------------------------------------------
		public EventBasicEditor(ComponentFile file, string tabText, string imageKey,
			AutoCompleteValueGatherer autoCompleteProvider, FieldGatherer fieldGatherer,
			PersonInformant personInformant)
			: base(file, tabText, imageKey)
		{
			InitializeComponent();
			Name = "EventEditor";

			_personInformant = personInformant;
			InitializeGrid(autoCompleteProvider, fieldGatherer);
			_status.Items.AddRange(Event.GetStatusNames().ToArray());

			SetBindingHelper(_binder);
			_autoCompleteHelper.SetAutoCompleteProvider(autoCompleteProvider);

			if (GenreDefinition.FactoryGenreDefinitions != null)
			{
				//add the ones in use, factory or otherwise
				var valueLists = autoCompleteProvider.GetValueLists(false);
				IEnumerable<string> list;
				if (valueLists.TryGetValue("genre", out list))
				{
					_genre.Items.AddRange(list.ToArray());
					_genre.Items.Add("-----");
				}

				// Add the rest of the factory defaults
				_genre.Items.AddRange(GenreDefinition.FactoryGenreDefinitions.ToArray());

				var genre = GenreDefinition.FactoryGenreDefinitions.ToArray().FirstOrDefault(x => x.Id == _binder.GetValue("genre"));
				_genre.SelectedItem = (genre ?? GenreDefinition.UnknownType);
			}

			_participants.JITListAcquisition += HandleParticipantJustInTimeListAcquisition;
		}

		/// ------------------------------------------------------------------------------------
		private IEnumerable<PickerPopupItem> HandleParticipantJustInTimeListAcquisition(object sender)
		{
			return from name in _personInformant.GetPeopleNames()
				orderby name
				select new PickerPopupItem { Text = name, ToolTipText = null };
		}

		/// ------------------------------------------------------------------------------------
		private void InitializeGrid(IMultiListDataProvider autoCompleteProvider,
			FieldGatherer fieldGatherer)
		{
			_gridViewModel = new FieldsValuesGridViewModel(_file, autoCompleteProvider,
				fieldGatherer, key => _file.FileType.GetIsCustomFieldId(key));

			_gridCustomFields = new FieldsValuesGrid(_gridViewModel);
			_gridCustomFields.Dock = DockStyle.Top;
			_panelGrid.AutoSize = true;
			_panelGrid.Controls.Add(_gridCustomFields);
		}

		/// ------------------------------------------------------------------------------------
		public override void SetComponentFile(ComponentFile file)
		{
			base.SetComponentFile(file);

			if (_gridViewModel != null)
				_gridViewModel.SetComponentFile(file);
		}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Provide special handling for persisting the value of the event type combo.
		///// </summary>
		///// ------------------------------------------------------------------------------------
		//private bool GetComboBoxValue(BindingHelper helper, Control boundControl, out string newValue)
		//{
		//    newValue = null;

		//    if (boundControl != _status)
		//        return false;

		//    newValue = _status.Text.Replace(' ', '_');
		//    return true;
		//}

		/// ------------------------------------------------------------------------------------
		private void HandleIdEnter(object sender, EventArgs e)
		{
			// Makes sure the id's label is also visible when the id field gains focus.
			AutoScrollPosition = new Point(0, 0);
		}

		private void HandleParticipantJustInTimeListAcquisition(object sender, System.ComponentModel.CancelEventArgs e)
		{

		}
	}
}
