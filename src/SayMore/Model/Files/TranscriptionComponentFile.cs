
using System.Collections.Generic;
using System.Linq;
using SayMore.Transcription.Model;

namespace SayMore.Model.Files
{
	public class TranscriptionComponentFile : ComponentFile
	{
		public new delegate TranscriptionComponentFile Factory(
			ProjectElement parentElement, string pathToAnnotationFile);

		private EafFileHelper _eafFileHelper;

		/// ------------------------------------------------------------------------------------
		public TranscriptionComponentFile(ProjectElement parentElement,
			string pathToAnnotationFile, TextTranscriptionFileType fileType)
			: base(parentElement, pathToAnnotationFile, fileType, null, null, null)
		{
			// The annotated file is the same as the annotation file.
			PathToAnnotatedFile = pathToAnnotationFile;
			InitializeFileInfo();
			Load();
		}

		/// ------------------------------------------------------------------------------------
		public IEnumerable<ITier> Tiers { get; private set; }

		/// ------------------------------------------------------------------------------------
		public string GetAssociatedMediaFile()
		{
			return PathToAnnotatedFile.Substring(0, PathToAnnotatedFile.Length - ".transcription.eaf".Length);
		}

		/// ------------------------------------------------------------------------------------
		public override int DisplayIndentLevel
		{
			get { return 1; }
		}

		/// ------------------------------------------------------------------------------------
		public override void Save()
		{
			Save(PathToAnnotatedFile);
		}

		/// ------------------------------------------------------------------------------------
		public override void Save(string path)
		{
			_eafFileHelper.Save(Tiers.First(t => t.DataType == TierType.Audio ||
				t.DataType == TierType.Video), Tiers.Where(t => t.DataType == TierType.Text));
		}

		/// ------------------------------------------------------------------------------------
		public override void Load()
		{
			_eafFileHelper = new EafFileHelper(PathToAnnotatedFile, GetAssociatedMediaFile());
			Tiers = _eafFileHelper.GetTiers();
		}
	}
}
