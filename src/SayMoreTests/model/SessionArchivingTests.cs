using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;
using NUnit.Framework;
using Palaso.Reporting;
using Palaso.TestUtilities;
using SayMore;
using SayMore.Model;
using SayMore.Model.Files;
using SayMore.Properties;

namespace SayMoreTests.Utilities
{
	[TestFixture]
	public class SessionArchivingTests
	{
		private string _dummyProjectName;
		private TemporaryFolder _tmpFolder;
		private DummySession _session;
		private Mock<Person> _person;
		private Mock<PersonInformant> _personInformant;

		/// ------------------------------------------------------------------------------------
		[SetUp]
		public void Setup()
		{
			ErrorReport.IsOkToInteractWithUser = false;

			_dummyProjectName = "ArchiveHelperTestFolder";
			_tmpFolder = new TemporaryFolder(_dummyProjectName);

			CreateSessionAndMockedPerson();
		}

		/// ------------------------------------------------------------------------------------
		private void CreateSessionAndMockedPerson()
		{
			// Create a person
			var folder = Path.Combine(_tmpFolder.Path, "People");
			Directory.CreateDirectory(folder);
			folder = Path.Combine(folder, "ddo-person");
			Directory.CreateDirectory(folder);
			File.CreateText(Path.Combine(folder, "ddoPic.jpg")).Close();
			File.CreateText(Path.Combine(folder, "ddoVoice.wav")).Close();

			_person = new Mock<Person>();
			_person.Setup(p => p.FolderPath).Returns(Path.Combine(Path.Combine(_tmpFolder.Path, "People"), "ddo-person"));
			_person.Setup(p => p.Id).Returns("ddo-person");

			_personInformant = new Mock<PersonInformant>();
			_personInformant.Setup(i => i.GetPersonByName("ddo-person")).Returns(_person.Object);

			// Create a session
			var parentFolder = Path.Combine(_tmpFolder.Path, "Sessions");
			Directory.CreateDirectory(parentFolder);
			folder = Path.Combine(parentFolder, "ddo-session");
			Directory.CreateDirectory(folder);
			File.CreateText(Path.Combine(folder, "ddo.session")).Close();
			File.CreateText(Path.Combine(folder, "ddo.mpg")).Close();
			File.CreateText(Path.Combine(folder, "ddo.mp3")).Close();
			File.CreateText(Path.Combine(folder, "ddo.pdf")).Close();
			_session = new DummySession(parentFolder, "ddo-session", _personInformant.Object);

			// create a project file
			var projFileName = _dummyProjectName + Settings.Default.ProjectFileExtension;
			File.CreateText(Path.Combine(_tmpFolder.Path, projFileName)).Close();
		}

		/// ------------------------------------------------------------------------------------
		[TearDown]
		public void TearDown()
		{
			_tmpFolder.Dispose();
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		[Category("SkipOnTeamCity")]
		public void GetFilesToArchive_GetsCorrectListSize()
		{
			var files = _session.GetFilesToArchive();
			Assert.AreEqual(2, files.Count);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		[Category("SkipOnTeamCity")]
		public void GetFilesToArchive_GetsCorrectSessionFiles()
		{
			var files = _session.GetFilesToArchive();
			Assert.AreEqual(4, files[string.Empty].Item1.Count());

			var list = files[string.Empty].Item1.Select(Path.GetFileName).ToList();
			Assert.Contains("ddo.session", list);
			Assert.Contains("ddo.mpg", list);
			Assert.Contains("ddo.mp3", list);
			Assert.Contains("ddo.pdf", list);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		[Category("SkipOnTeamCity")]
		public void GetFilesToArchive_ParticipantFileDoNotExist_DoesNotCrash()
		{
			_session._participants = new[] { "ddo-person", "non-existant-person" };
			_session.GetFilesToArchive();
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		[Category("SkipOnTeamCity")]
		public void GetFilesToArchive_GetsCorrectPersonFiles()
		{
			var files = _session.GetFilesToArchive();
			Assert.AreEqual(2, files["ddo-person"].Item1.Count());
			var list = files["ddo-person"].Item1.Select(Path.GetFileName).ToList();
			Assert.Contains("ddoPic.jpg", list);
			Assert.Contains("ddoVoice.wav", list);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetTotalDurationOfSourceMedia_ThreeSourceMediaFiles_ReturnsTotalTime()
		{
			var sourceRoleArray = new[] { ApplicationContainer.ComponentRoles.First(r => r.Id == ComponentRole.kSourceComponentRoleId) };

			var sourceMediaFile1 = new Mock<ComponentFile>();
			sourceMediaFile1.Setup(f => f.DurationSeconds).Returns(new TimeSpan(0, 50, 10));
			sourceMediaFile1.Setup(f => f.GetAssignedRoles()).Returns(sourceRoleArray);
			var sourceMediaFile2 = new Mock<ComponentFile>();
			sourceMediaFile2.Setup(f => f.DurationSeconds).Returns(new TimeSpan(3, 20, 3));
			sourceMediaFile2.Setup(f => f.GetAssignedRoles()).Returns(sourceRoleArray);
			var sourceMediaFile3 = new Mock<ComponentFile>();
			sourceMediaFile3.Setup(f => f.DurationSeconds).Returns(new TimeSpan(0, 4, 27));
			sourceMediaFile3.Setup(f => f.GetAssignedRoles()).Returns(sourceRoleArray);

			_session._mediaFiles = new[] { sourceMediaFile1.Object, sourceMediaFile2.Object, sourceMediaFile3.Object };
			Assert.AreEqual(new TimeSpan(4, 14, 40), _session.GetTotalDurationOfSourceMedia());
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetTotalDurationOfSourceMedia_TwoSourceMediaFilesAndOneNonSourceMediaFile_ReturnsTotalTime()
		{
			var sourceRoleArray = new[] { ApplicationContainer.ComponentRoles.First(r => r.Id == ComponentRole.kSourceComponentRoleId) };

			var sourceMediaFile1 = new Mock<ComponentFile>();
			sourceMediaFile1.Setup(f => f.DurationSeconds).Returns(new TimeSpan(0, 50, 10));
			sourceMediaFile1.Setup(f => f.GetAssignedRoles()).Returns(sourceRoleArray);
			var sourceMediaFile2 = new Mock<ComponentFile>();
			sourceMediaFile2.Setup(f => f.DurationSeconds).Returns(new TimeSpan(3, 20, 3));
			sourceMediaFile2.Setup(f => f.GetAssignedRoles()).Returns(new[]
				{
					ApplicationContainer.ComponentRoles.First(r => r.Id == ComponentRole.kConsentComponentRoleId),
					ApplicationContainer.ComponentRoles.First(r => r.Id == ComponentRole.kSourceComponentRoleId)
				});
			var sourceMediaFile3 = new Mock<ComponentFile>();
			sourceMediaFile3.Setup(f => f.DurationSeconds).Returns(new TimeSpan(0, 4, 27));
			sourceMediaFile3.Setup(f => f.GetAssignedRoles()).Returns(new[]
				{
					ApplicationContainer.ComponentRoles.First(r => r.Id == ComponentRole.kConsentComponentRoleId)
				});

			_session._mediaFiles = new[] { sourceMediaFile1.Object, sourceMediaFile2.Object, sourceMediaFile3.Object };
			Assert.AreEqual(new TimeSpan(4, 10, 13), _session.GetTotalDurationOfSourceMedia());
		}

		#region IMDI Archiving Tests

		[Test]
		public void GetProjectName_ReturnsCorrectProjectName()
		{
			var projname = _session.GetProjectName();
			Assert.AreEqual(_dummyProjectName, projname);
		}

		#endregion
	}

	public class DummySession : Session
	{
		public string[] _participants;
		private readonly Mock<ProjectElementComponentFile> _metaFile = new Mock<ProjectElementComponentFile>();
		public ComponentFile[] _mediaFiles;

		public DummySession(string parentFolder, string id, PersonInformant personInformant) : base(parentFolder, id, null, new SessionFileType(() => null, () => null),
				MakeComponent, new XmlFileSerializer(null), (w, x, y, z) =>
					new ProjectElementComponentFile(w, x, y, z, FieldUpdater.CreateMinimalFieldUpdaterForTests(null)),
					ApplicationContainer.ComponentRoles, personInformant)
		{
			_participants = new[] {"ddo-person"};
			_metaFile.Setup(m => m.GetStringValue("title", null)).Returns("StupidSession");
		}

		public override IEnumerable<string> GetAllParticipants()
		{
			return _participants;
		}

		public override ProjectElementComponentFile MetaDataFile
		{
			get { return _metaFile.Object; }
		}

		public override ComponentFile[] GetComponentFiles()
		{
			lock (this)
			{
				// Return a copy of the list to guard against changes
				// on another thread (i.e., from the FileSystemWatcher)
				if (_componentFiles == null)
				{
					_componentFiles = new HashSet<ComponentFile>();

					// This is the actual person or session data
					_componentFiles.Add(MetaDataFile);

					foreach (ComponentFile componentFile in _mediaFiles)
						_componentFiles.Add(componentFile);
				}
				return _componentFiles.ToArray();
			}
		}

		private static ComponentFile MakeComponent(ProjectElement parentElement, string pathtoannotatedfile)
		{
			return ComponentFile.CreateMinimalComponentFileForTests(parentElement, pathtoannotatedfile);
		}
	}
}