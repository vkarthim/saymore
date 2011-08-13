using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NAudio.Wave;
using SayMore.AudioUtils;
using SayMore.Model.Files;
using SayMore.Properties;
using SayMore.Transcription.Model;
using SayMore.UI.MediaPlayer;

namespace SayMore.Transcription.UI
{
	public class OralAnnotationRecorderViewModel : IDisposable
	{
		public enum State
		{
			Idle,
			PlayingOriginal,
			PlayingAnnotation,
			Recording
		}

		public event EventHandler PlaybackEnded;

		public int CurrentSegmentNumber { get; private set; }

		private readonly MediaPlayerViewModel _origPlayerViewModel;
		private readonly ITimeOrderSegment[] _segments;
		private AudioRecorder _annotationRecorder;
		private AudioPlayer _annotationPlayer;
		private readonly string _pathToAnnotationsFolder;
		public OralAnnotationType _annotationType;
		public string _originalRecordingPath;

		public Control MicLevelDisplayControl { get; set; }
		public TrackBar MicLevelChangeControl { get; set; }

		/// ------------------------------------------------------------------------------------
		public OralAnnotationRecorderViewModel(OralAnnotationType annotationType,
			TimeOrderTier tier)
		{
			_origPlayerViewModel = new MediaPlayerViewModel();
			_origPlayerViewModel.SetVolume(100);
			_origPlayerViewModel.PlaybackEnded += delegate
			{
				if (PlaybackEnded != null)
					PlaybackEnded(true, EventArgs.Empty);
			};

			_annotationType = annotationType;
			_originalRecordingPath = tier.MediaFileName;
			_segments = tier.GetAllSegments().Cast<ITimeOrderSegment>().ToArray();
			_pathToAnnotationsFolder = tier.MediaFileName + "_Annotations";
			SetCurrentSegmentNumber(0);
		}

		/// ------------------------------------------------------------------------------------
		public void Dispose()
		{
			CloseAnnotationPlayer();
			CloseAnnotationRecorder();
		}

		/// ------------------------------------------------------------------------------------
		private void CloseAnnotationPlayer()
		{
			if (_annotationPlayer != null)
				_annotationPlayer.Dispose();

			_annotationPlayer = null;
		}

		/// ------------------------------------------------------------------------------------
		private void CloseAnnotationRecorder()
		{
			if (_annotationRecorder != null)
				_annotationRecorder.Dispose();

			_annotationRecorder = null;
		}

		/// ------------------------------------------------------------------------------------
		public int SegmentCount
		{
			get { return _segments.Length; }
		}

		/// ------------------------------------------------------------------------------------
		public string GetPathToAnnotationFileForSegment(int segmentNumber)
		{
			var segment = _segments[segmentNumber];

			return Path.Combine(_pathToAnnotationsFolder,
				string.Format(Settings.Default.OralAnnotationSegmentFileAffix,
					segment.Start, segment.Stop, _annotationType));
		}

		/// ------------------------------------------------------------------------------------
		public string GetPathToCurrentAnnotationFile()
		{
			return GetPathToAnnotationFileForSegment(CurrentSegmentNumber);
		}

		/// ------------------------------------------------------------------------------------
		public bool SetCurrentSegmentNumber(int segmentNumber)
		{
			Stop();
			CloseAnnotationRecorder();

			bool incremented = (CurrentSegmentNumber < segmentNumber);
			CurrentSegmentNumber = segmentNumber;

			_annotationRecorder = new AudioRecorder(WaveFileUtils.GetDefaultWaveFormat(1));
			_annotationRecorder.SetRecordingLevelChangeControl(MicLevelChangeControl);
			_annotationRecorder.SetRecordingLevelDisplayControl(MicLevelDisplayControl);

			var segment = _segments[CurrentSegmentNumber];
			_origPlayerViewModel.LoadFile(_originalRecordingPath, segment.Start, segment.GetLength());
			InitializeAnnotationPlayerModel();

			return incremented;
		}

		/// ------------------------------------------------------------------------------------
		private void InitializeAnnotationPlayerModel()
		{
			CloseAnnotationPlayer();

			var filename = GetPathToCurrentAnnotationFile();
			if (File.Exists(filename))
			{
				var fi = new FileInfo(filename);
				if (fi.Length == 0)
				{
					fi.Delete();
					return;
				}

				_annotationPlayer = new AudioPlayer();
				_annotationPlayer.LoadFile(filename);
				_annotationPlayer.Stopped += delegate
				{
					if (PlaybackEnded != null)
						PlaybackEnded(false, EventArgs.Empty);
				};
			}
		}

		/// ------------------------------------------------------------------------------------
		public State GetState()
		{
			if (_annotationRecorder != null && _annotationRecorder.RecordingState == RecordingState.Recording)
				return State.Recording;

			if (_origPlayerViewModel.HasPlaybackStarted)
				return State.PlayingOriginal;

			if (_annotationPlayer != null && _annotationPlayer.PlaybackState == PlaybackState.Playing)
				return State.PlayingAnnotation;

			return State.Idle;
		}

		/// ------------------------------------------------------------------------------------
		public void EraseAnnotation()
		{
			Stop();
			CloseAnnotationPlayer();
			var filename = GetPathToCurrentAnnotationFile();
			ComponentFile.WaitForFileRelease(filename);
			File.Delete(filename);
			InitializeAnnotationPlayerModel();
		}

		/// ------------------------------------------------------------------------------------
		public void BeginRecording()
		{
			Stop();
			_annotationRecorder.BeginRecording(GetPathToCurrentAnnotationFile());
		}

		/// ------------------------------------------------------------------------------------
		public void PlayAnnotation()
		{
			Stop();
			_annotationPlayer.Play();
		}

		/// ------------------------------------------------------------------------------------
		public void PlayOriginalRecording()
		{
			Stop();
			_origPlayerViewModel.Play();
		}

		/// ------------------------------------------------------------------------------------
		public void Stop()
		{
			if (_annotationRecorder != null && _annotationRecorder.RecordingState == RecordingState.Recording)
			{
				_annotationRecorder.Stop();
				InitializeAnnotationPlayerModel();
			}

			if (_annotationPlayer != null && _annotationPlayer.PlaybackState != PlaybackState.Stopped)
				_annotationPlayer.Stop();

			if (_origPlayerViewModel != null && _origPlayerViewModel.HasPlaybackStarted)
				_origPlayerViewModel.Stop();
		}

		#region Properties for enabling, showing and hiding player buttons.
		/// ------------------------------------------------------------------------------------
		public bool IsRecording
		{
			get { return _annotationRecorder != null && _annotationRecorder.RecordingState == RecordingState.Recording; }
		}

		/// ------------------------------------------------------------------------------------
		public bool ShouldRecordButtonBeVisible
		{
			get { return IsRecording || !ShouldListenToAnnotationButtonBeVisible; }
		}

		/// ------------------------------------------------------------------------------------
		public bool ShouldListenToAnnotationButtonBeVisible
		{
			get
			{
				return File.Exists(GetPathToCurrentAnnotationFile()) && _annotationPlayer != null &&
					(_annotationRecorder == null || _annotationRecorder.RecordingState != RecordingState.Recording);
			}
		}

		/// ------------------------------------------------------------------------------------
		public bool ShouldEraseAnnotationButtonBeVisible
		{
			get { return DoesAnnotationExist; }
		}

		/// ------------------------------------------------------------------------------------
		public bool ShouldEraseAnnotationButtonBeEnabled
		{
			get
			{
				return DoesAnnotationExist && !IsRecording && !_origPlayerViewModel.HasPlaybackStarted &&
					(_annotationPlayer == null || _annotationPlayer.PlaybackState != PlaybackState.Playing);
			}
		}

		/// ------------------------------------------------------------------------------------
		public bool DoesAnnotationExist
		{
			get { return (File.Exists(GetPathToCurrentAnnotationFile())); }
		}

		#endregion
	}
}
