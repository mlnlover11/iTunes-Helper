﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Microsoft.WindowsAPICodePack.Taskbar;

namespace MediaPlayer.Tasks
{
    class LyricsFetchingTask
    {
        static Lyrics.LyricsFetchManager man = new Lyrics.LyricsFetchManager(null);
        static LyricsFetchingTask()
        {
            man.RegisterSource(new Lyrics.LyricsSourceLyrdb());           // LyricsFetcher
            //man.RegisterSource(new Lyrics.LyricsSourceLyricsPlugin());  // LyricsFetcher, BROKEN
            //man.RegisterSource(new Lyrics.LyricsSourceLyricsFly());     // LoDC, broken
            man.RegisterSource(new Lyrics.LyricsSourceAZLyrics());        // iTuner
            man.RegisterSource(new Lyrics.LyricsSourceLyrics007());       // iTuner
            man.RegisterSource(new Lyrics.LyricsSourceMP3Lyrics());       // iTuner
            man.RegisterSource(new Lyrics.LyricsSourceAbsoluteLyrics());  // LoDC
        }

        iTunesLib.IITPlaylist t;
        string subText, text;
        int foundLyrics = 0;
        int current, total;
        bool finished;
        bool cancel = false;
        BackgroundWorker bg = new BackgroundWorker();
        TaskbarManager tm = TaskbarManager.Instance;
        int currentTrack = 0;
        int threads = 0;

        public LyricsFetchingTask(iTunesLib.IITPlaylist t)
        {
            this.t = t;
            current = 0;
            total = 1;
            finished = false;
            subText = "";
            text = string.Format("Getting lyrics for {0} songs", t.Tracks.Count);
        }

        void bg_DoWork(object sender, DoWorkEventArgs e)
        {
            subText = "Preparing to fetch lyrics for " + t.Tracks.Count + " tracks...";
            try
            {
                man.t = t;
                man.OnTrackSkipped += onSkipTrack;
                man.OnNewThread += delegate { threads++; };
                man.OnRemoveThread += delegate { threads--; };
                man.OnTrackFinished += delegate { currentTrack++; };
                man.OnStatusChanged += new EventHandler<Lyrics.LyricsFetchManager.LyricEventArgs>(man_OnStatusChanged);
                total = t.Tracks.Count * man.Sources.Count;
                tm.SetProgressValue(0, total);
                man.Start();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
        }

        void man_OnStatusChanged(object sender, Lyrics.LyricsFetchManager.LyricEventArgs e)
        {
            tm.SetProgressValue(current, total);
            if (e.LyricsFound)
            {
                subText = String.Format("Lyrics for '{1}' found on source '{0}'!", e.Source.Name, e.Track.Name);
                foundLyrics++;
            }
            else
            {
                subText = String.Format("Lyrics for '{1}' not found on source '{0}'.", e.Source.Name, e.Track.Name);
            }
            text = string.Format("Getting lyrics for {0} songs, {1} total lyrics found, {2} tracks scanned, {3} running threads", t.Tracks.Count/* - foundLyrics*/, foundLyrics, currentTrack, threads);
            if (OnStatusChanged != null)
                OnStatusChanged(null, null);
            current++;
        }
        
        void onSkipTrack(object sender, EventArgs e)
        {
            current += man.Sources.Count;
            //Debug.WriteLine("+1 track finished!");
            currentTrack++;
        }
        
        public event EventHandler OnStatusChanged;

        public void Run()
        {
            bg.DoWork += new DoWorkEventHandler(bg_DoWork);
            bg.RunWorkerCompleted += delegate { OnDone(null, null); };
            bg.RunWorkerAsync();
        }

        public bool IsCancelable
        {
            get { return true; }
        }

        public void Cancel()
        {
            cancel = true;
        }

        public int TotalAmount
        {
            get { return total; }
        }

        public int CurrentProgressAmount
        {
            get { return current; }
        }

        public string Text
        {
            get { return text; }
        }

        public string SubText
        {
            get { return subText; }
        }
        
        public event EventHandler OnDone;

        public bool IsFinished
        {
            get { return finished; }
        }
    }
}
