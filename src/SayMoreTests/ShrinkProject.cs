using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.Code;
using Palaso.CommandLineProcessing;


namespace SayMoreTests
{
	/// <summary>
	/// I really need this feature now, so I'm just going to hack it into a test which does what I want
	/// and someday we can make it a real feature
	/// </summary>
	[TestFixture]
	public class ShrinkProject
	{
		private ConsoleProgress _progress;

		[SetUp]
		public void Setup()
		{
		   _progress = new ConsoleProgress();
			_progress.ShowVerbose = true;
		}

		[Test]
		public void ShrinkFolder()
		{
			var destinationFolder = @"C:\dev\temp\shrunkEdolo";
			if(Directory.Exists(destinationFolder))
			{
				Directory.Delete(destinationFolder,true);
			}

			CloneAndShrinkProject( @"C:\Users\John\Documents\Language Data\edolo\SmallEdolo", destinationFolder);
		}

		public void CloneAndShrinkProject(string originalProjectFolder, string destinationFolder)
		{
			RequireThat.Directory(destinationFolder).DoesNotExist();
			RequireThat.Directory(originalProjectFolder).Exists();

			Directory.CreateDirectory(destinationFolder);
			//we don't currently process anything at this level, just copy it
			foreach (var original in Directory.GetFiles(originalProjectFolder))
			{
				File.Copy(original, Path.Combine(destinationFolder, Path.GetFileName(original)));
			}

			foreach (var directory in Directory.GetDirectories(originalProjectFolder))
			{
				var currentDestSubDirectory = Path.Combine(destinationFolder, Path.GetFileName(directory));
				Directory.CreateDirectory(currentDestSubDirectory);

				//we don't currently process anything at this level, just copy it
				foreach (var original in Directory.GetFiles(directory))
				{
					File.Copy(original, Path.Combine(currentDestSubDirectory, Path.GetFileName(original)));
				}

				foreach (var sub in Directory.GetDirectories(directory))
				{
					var currentDestSubSubDirectory = Path.Combine(currentDestSubDirectory, Path.GetFileName(sub));
					Directory.CreateDirectory(currentDestSubSubDirectory);
					foreach (var original in Directory.GetFiles(sub))
					{
						if (original.Contains("-small"))
							continue;
						if (original.EndsWith(".meta"))
							continue;
						if (original.EndsWith(".smd"))//artifact of old version
							continue;

						var extension = Path.GetExtension(original).ToLower();
						string newPath=string.Empty;
						var newPathRoot =Path.Combine(currentDestSubSubDirectory, Path.GetFileNameWithoutExtension(original));
						switch (extension)
						{
							default:
								File.Copy(original, Path.Combine(currentDestSubSubDirectory, Path.GetFileName(original)));
								break;
							case ".mov":
							case ".avi":
								newPath = ShrinkVideo(original, newPathRoot);
								break;
							case ".jpg":
								//newPath = ShrinkPicture(original, newPathRoot);
								break;
							case ".wav":
							case ".mp3":
							   // newPath = ShrinkAudio(original, newPathRoot);
								break;
							case ".meta":
								break;

						}
						if (!string.IsNullOrEmpty(newPath) && File.Exists(newPath) && File.Exists(original + ".meta"))
							File.Move(original + ".meta", newPath + ".meta");
					}
				}
			}
		}

		private string ShrinkAudio(string original, string newPathRoot)
		{
			Debug.WriteLine("ShrinkAudio " + original);

			var extension = Path.GetExtension(original);
			var newPath = newPathRoot+".mp3";
			if(File.Exists(newPath))
				File.Delete(newPath);

			Palaso.Media.FFmpegRunner.MakeLowQualityCompressedAudio(original, newPath,
																   _progress);
			return newPath;

		}

		private string ShrinkPicture(string original, string newPathRoot)
		{
			Debug.WriteLine("ShrinkPicture " + original);
			var extension = Path.GetExtension(original);
			var newPath = newPathRoot+".jpg";
			if (File.Exists(newPath))
				File.Delete(newPath);
			Palaso.Media.FFmpegRunner.MakeLowQualitySmallPicture(original, newPath,
																   _progress);
			return newPath;
		}

		private string ShrinkVideo(string original, string newPathRoot)
		{
			Debug.WriteLine("ShrinkVIdeo "+original);

			var extension = Path.GetExtension(original);

			var newPath = newPathRoot + ".mp4";
			if (File.Exists(newPath))
				File.Delete(newPath);
			var result = Palaso.Media.FFmpegRunner.MakeLowQualitySmallVideo(original, newPath,
																	_progress);
			return newPath;
		}
	}
}