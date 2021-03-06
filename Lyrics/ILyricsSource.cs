using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Text.RegularExpressions;

namespace MediaPlayer.Lyrics
{
    /// <summary>
    /// A LyricsSource represents a mechanism for finding the lyrics to a song.
    /// </summary>
    /// <remarks>
    /// Sources must be thread safe, since they will be used to fetch different songs at the same
    /// time in different threads.
    /// </remarks>
    public interface ILyricsSource
    {

        /// <summary>
        /// Gets the name of this source
        /// </summary>
        string Name
        {
            get;
        }

        /// <summary>
        /// Fetch the lyrics for the given song
        /// </summary>
        /// <param name="s">The song whose lyrics are to be fetched</param>
        /// <returns>The lyrics or an empty string if the lyrics could not be found</returns>
        string GetLyrics(iTunesLib.IITFileOrCDTrack s);
    }
}