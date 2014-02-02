using System;
using System.IO;
using System.Net;
using UnityEngine;
using System.Threading;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;
using System.Collections.Generic;
//Written by GibsonBethke
//Thank you for everything, Jesus, you are so awesome!

public class Song
{
	
	public String name;

	public Album album;
	public Artist artist;
	public Genre genre;

	public String format;
	public String downloadLink;
	public String supportLink;
	
	public String releaseDate;
}

public class Album : IEquatable<Album>
{
	
	public String name;
	public List<Song> songs;
	
	public Album (String Name, List<Song> Songs)
	{
		
		this.name = Name;
		this.songs = Songs;
	}
	
	public bool Equals(Album other)
	{
		if (other == null) return false;
		return (this.name.Equals(other.name));
	}
}

public class Artist : IEquatable<Artist>
{
	
	public String name;
	public List<Song> songs;

	public Artist (String Name, List<Song> Songs)
	{

		this.name = Name;
		this.songs = Songs;
	}

	public bool Equals(Artist other)
	{
		if (other == null) return false;
		return (this.name.Equals(other.name));
	}
}

public class Genre : IEquatable<Genre>
{

	public String name;
	public List<Song> songs;

	public Genre (String Name, List<Song> Songs)
	{
		
		this.name = Name;
		this.songs = Songs;
	}
	
	public bool Equals(Genre other)
	{
		if (other == null) return false;
		return (this.name.Equals(other.name));
	}	
}

public class OnlineMusicBrowser : MonoBehaviour
{
	
	StartupManager startupManager;
	MusicViewer musicViewer;
	PaneManager paneManager;
//	LoadingImage loadingImage;
	DownloadManager downloadManager;

	#region OMBVariables

	public GUISkin guiSkin;
	GUIStyle labelStyle;
	GUIStyle buttonStyle;
	GUIStyle infoLabelStyle;
	
	public Texture2D guiHover;
	internal bool showUnderlay = false;
	
	internal bool showOnlineMusicBrowser = false;
	bool drawGUI = false;
	bool drawOMB = false;

	Vector2 scrollPosition;
	internal Rect onlineMusicBrowserPosition = new Rect(0, 0, 800, 600);
	internal string onlineMusicBrowserTitle;
	
	internal bool showDownloadList = false;

	#region Lists
	
	string[] allSongs;
	List<Song> allRecentList = new List<Song>();
	List<Song> allSongsList = new List<Song>();
	List<Album> allAlbumsList = new List<Album>();
	List<Artist> allArtistsList = new List<Artist>();
	List<Genre> allGenresList = new List<Genre>();
	List<Song> specificSort = new List<Song>();
	
	#endregion
	
	int sortBy = 4;
	string currentPlace = "Recent";

	#endregion
	
	#region DownloadInformation
	
	Song songInfoOwner;
	
	public WebClient client;
	
	Uri url;
	Song song;
	string downloadButtonText;
	
	string currentDownloadSize;
	string currentDownloadPercentage;
	
	bool showSongInformation = false;
	bool downloading = false;
	
	Song downloadingSong;
	
	#endregion
	

	void Start ()
	{

		startupManager = GameObject.FindGameObjectWithTag ( "Manager" ).GetComponent<StartupManager>();

		musicViewer = GameObject.FindGameObjectWithTag ( "MusicViewer" ).GetComponent<MusicViewer>();
//		loadingImage = GameObject.FindGameObjectWithTag ( "LoadingImage" ).GetComponent<LoadingImage>();
		paneManager = GameObject.FindGameObjectWithTag ("Manager").GetComponent<PaneManager>();

		onlineMusicBrowserPosition.width = Screen.width;
		onlineMusicBrowserPosition.height = Screen.height;
		onlineMusicBrowserPosition.x = onlineMusicBrowserPosition.width + onlineMusicBrowserPosition.width / 4;
		
		labelStyle = new GUIStyle ();
		labelStyle.alignment = TextAnchor.MiddleCenter;
		labelStyle.fontSize = 32;
		
		infoLabelStyle = new GUIStyle ();
		infoLabelStyle.alignment = TextAnchor.MiddleLeft;
		infoLabelStyle.fontSize = 16;
		
		buttonStyle = new GUIStyle ();
		buttonStyle.fontSize = 16;
		buttonStyle.alignment = TextAnchor.MiddleCenter;
		buttonStyle.border = new RectOffset ( 6, 6, 4, 4 );
		buttonStyle.hover.background = guiHover;
	}
	

	void StartOMB ()
	{
		
		allSongs = startupManager.allSongs;
		Thread refreshThread = new Thread (SortAvailableDownloads);
		refreshThread.Start();
	}
	
	
	void SortAvailableDownloads()
	{

		int i = 0;

		while (i < allSongs.Length)
		{
			
			i += 9;
			Song song = new Song();
			song.name = allSongs [i - 8];
			
			Album tempAlbum = new Album(allSongs [i - 7], new List<Song>());
			if(allAlbumsList.Contains (tempAlbum))
			{
				
				Album addToAlbum = allAlbumsList.Find(Album => Album.name == tempAlbum.name);
				addToAlbum.songs.Add (song);
				song.album = tempAlbum;
			} else {
				
				tempAlbum.songs.Add (song);
				allAlbumsList.Add (tempAlbum);
				song.album = tempAlbum;
			}
			
			Artist tempArtist = new Artist(allSongs [i - 6], new List<Song>());
			if(allArtistsList.Contains (tempArtist))
			{
				
				Artist addToArtist = allArtistsList.Find(Artist => Artist.name == tempArtist.name);
				addToArtist.songs.Add (song);
				song.artist = tempArtist;
			} else {
				
				tempArtist.songs.Add (song);
				allArtistsList.Add (tempArtist);
				song.artist = tempArtist;
			}
			
			Genre tempGenre = new Genre(allSongs [i - 5], new List<Song>());
			if(allGenresList.Contains (tempGenre))
			{
				
				Genre addToGenre = allGenresList.Find(Genre => Genre.name == tempGenre.name);
				addToGenre.songs.Add (song);
				song.genre = tempGenre;
			} else {
				
				tempGenre.songs.Add (song);
				allGenresList.Add (tempGenre);
				song.genre = tempGenre;
			}
			
			
			song.format = allSongs [i - 4];
			song.downloadLink = allSongs [i - 3];
			song.supportLink = allSongs [i - 2];
			song.releaseDate = allSongs [i - 1];
			
			allSongsList.Add ( song );
			allRecentList.Add ( song );
		}

		allSongsList.Sort (( a, b ) => a.name.CompareTo ( b.name ));
		allAlbumsList.Sort (( a, b ) => a.name.CompareTo ( b.name ));
		allArtistsList.Sort (( a, b ) => a.name.CompareTo ( b.name ));
		allGenresList.Sort (( a, b ) => a.name.CompareTo ( b.name ));
		allRecentList.Reverse ();

		paneManager.loading = false;
		
		if ( paneManager.currentPane == PaneManager.pane.onlineMusicBrowser )
		{
			
			musicViewer.tempEnableOMB = 1.0F;
			startupManager.ombEnabled = true;
		}
	}
	
	
	void OnGUI ()
	{
			
		if ( drawGUI == true )
		{
		
			GUI.skin = guiSkin;
		
			if ( paneManager.loading == false )
				onlineMusicBrowserPosition = GUI.Window ( 1, onlineMusicBrowserPosition, OnlineMusicBrowserPane, onlineMusicBrowserTitle );
		}
		
		if ( showOnlineMusicBrowser == true )
		{
			
			drawGUI = true;
		} else {
			
			drawGUI = false;
		}
	}
	

	void OnlineMusicBrowserPane (int wid)
	{
		
		if ( drawOMB == true )
		{
	
			GUILayout.Space ( onlineMusicBrowserPosition.width / 8 );
			GUILayout.BeginHorizontal ();
	
			if (GUILayout.Button ("Name"))
			{
	
				sortBy = 0;
				currentPlace = "Name";
			}
	
			if (GUILayout.Button ("Albums"))
			{
	
				sortBy = 1;
				currentPlace = "Albums";
			}
	
			if (GUILayout.Button ("Artists"))
			{
	
				sortBy = 2;
				currentPlace = "Artists";
			}
	
			if (GUILayout.Button ("Genres"))
			{
	
				sortBy = 3;
				currentPlace = "Genres";
			}
	
			if (GUILayout.Button ("Recent"))
			{
	
				sortBy = 4;
				currentPlace = "Recent";
			}
	
			GUILayout.EndHorizontal ();
			GUILayout.Space ( 5 );
			GUILayout.BeginHorizontal ();
			GUILayout.Space ( onlineMusicBrowserPosition.width / 2 - 300  );
	
			scrollPosition = GUILayout.BeginScrollView ( scrollPosition, GUILayout.Width( 600 ), GUILayout.Height (  onlineMusicBrowserPosition.height - ( onlineMusicBrowserPosition.height / 4 + 56 )));
			GUILayout.Box ( "Current Sort: " + currentPlace );
			
			switch (sortBy)
			{		
				
				case 0:
				specificSort = allSongsList;
				sortBy = 5;
				break;
				
				case 1:
				foreach ( Album album in allAlbumsList )
				{
	
					if ( GUILayout.Button ( album.name ))
					{
	
						specificSort = album.songs;
						currentPlace = "Albums > " + album.name;
						sortBy = 5;
					}
				}
				break;
	
				case 2:
				foreach ( Artist artist in allArtistsList )
				{
					
					if ( GUILayout.Button ( artist.name ))
					{
	
						specificSort = artist.songs;
						currentPlace = "Artists > " + artist.name;
						sortBy = 5;
					}
				}
				break;
	
				case 3:
				foreach ( Genre genre in allGenresList )
				{
					
					if ( GUILayout.Button ( genre.name ))
					{
	
						specificSort = genre.songs;
						currentPlace = "Genres > " + genre.name;
						sortBy = 5;
					}
				}
				break;
				
				case 4:
				specificSort = allRecentList;
				sortBy = 5;
				break;
				
				case 5:
				foreach ( Song song in specificSort )
				{
					
					if ( GUILayout.Button ( song.name ))
					{
						
						if ( showSongInformation == false )
						{
							
							if ( song.downloadLink.StartsWith ( "|" ) == true )
							{
									
								url = null;
								downloadButtonText = song.downloadLink.Substring ( 1 );
							} else if ( song.downloadLink.StartsWith ( "h" ) == true )
							{
									
								url = new Uri ( song.downloadLink );
								downloadButtonText = "Download";
							}
		
							currentDownloadPercentage = "";
							currentDownloadSize = "Loading";
								
							Thread getInfoThread = new Thread (GetInfoThread);
							getInfoThread.Priority = System.Threading.ThreadPriority.AboveNormal;
							getInfoThread.Start();
							
							showSongInformation = true;
							songInfoOwner = song;
						} else {
							
							showSongInformation = false;
							GUI.FocusWindow ( 1 );
							GUI.BringWindowToFront ( 1 );
						}
					}
					
					if ( showSongInformation == true )
					{
						
						if ( songInfoOwner == song )
						{
						
							if ( downloading == false )
							{
					
								if ( GUILayout.Button ( downloadButtonText, buttonStyle ) && url != null )
								{
									
									UnityEngine.Debug.Log ( "StartingDownload Stage 1" );
									
									downloadingSong = song;
									
									currentDownloadPercentage = " - Processing";
									
									try
									{
										
										using ( client = new WebClient ())
										{
						 
							        		client.DownloadFileCompleted += new AsyncCompletedEventHandler ( DownloadFileCompleted );
							
							        		client.DownloadProgressChanged += new DownloadProgressChangedEventHandler( DownloadProgressCallback );
											
							        		client.DownloadFileAsync ( url, startupManager.tempPath + Path.DirectorySeparatorChar + song.name + "." + song.format );
										}
									} catch ( Exception error ) {
										
										UnityEngine.Debug.Log ( error );
									}
												
									downloading = true;

								}
							}
							
							if ( downloading == true )
							{

								if ( GUILayout.Button ( "Cancel Download", buttonStyle ))
								{
									
									client.CancelAsync ();
								}
							}
							
							if ( song.supportLink != "NONE" )
							{
								
								if ( GUILayout.Button ( "Support Artist", buttonStyle ))
									Process.Start ( song.supportLink );
							}
								
							GUILayout.Label ( "Download size: ~" + currentDownloadSize + currentDownloadPercentage );
					
							GUILayout.Label ("Name: " + song.name, infoLabelStyle );
							GUILayout.Label ("Artist: " + song.artist.name, infoLabelStyle );
							GUILayout.Label ("Album: " + song.album.name, infoLabelStyle );
							GUILayout.Label ("Genre: " + song.genre.name, infoLabelStyle );
							GUILayout.Label ("Format: " + song.format, infoLabelStyle );
							GUILayout.Label ("Released: " + song.releaseDate, infoLabelStyle );
						}
					}
				}		
				
				break;
					
				default:
				break;
					
			}
			
			GUILayout.EndScrollView ();
			GUILayout.EndHorizontal ();
			
		} else {
			
			GUI.Label ( new Rect ( 10, onlineMusicBrowserPosition.height / 4, onlineMusicBrowserPosition.width - 20, 128 ), "The OnlineMusicBrowser has been disabled!", labelStyle );
			if ( GUI.Button ( new Rect ( onlineMusicBrowserPosition.width/2 - 160, onlineMusicBrowserPosition.height / 2, 320, 64 ), "Enable OnlineMusicBrowser" ))
			{
				
				startupManager.SendMessage ( "RefreshOMB" );
			}
		}
		
		if ( startupManager.ombEnabled == true )
		{
			
			drawOMB = true;
		} else {
			
			drawOMB = false;
		}
	}
	
	void GetInfoThread ()
	{
	
		try
		{
					
			System.Net.WebRequest req = System.Net.HttpWebRequest.Create ( url );
			req.Method = "HEAD";
			System.Net.WebResponse resp = req.GetResponse();
			currentDownloadSize = Math.Round ( float.Parse ( resp.Headers.Get ( "Content-Length" )) / 1024 / 1024, 2 ).ToString () + "MB";
		} catch ( Exception e ) {
			
			UnityEngine.Debug.Log ( e );
		}
	}
	
	
	void DownloadFileCompleted ( object sender, AsyncCompletedEventArgs end )
	{
		
		if ( downloading == true )
		{
			
			if ( end.Cancelled == true )
			{
				
//				UnityEngine.Debug.Log ( "WAS cancelled" );
				
				File.Delete ( startupManager.tempPath + Path.DirectorySeparatorChar + downloadingSong.name + "." + downloadingSong.format );
			} else {
				
//				UnityEngine.Debug.Log ( "Was NOT cancelled" );
				
				if ( File.Exists ( musicViewer.mediaPath + Path.DirectorySeparatorChar + downloadingSong.name + "." + downloadingSong.format ))
					File.Delete ( musicViewer.mediaPath + Path.DirectorySeparatorChar + downloadingSong.name + "." + downloadingSong.format );
				
				File.Move ( startupManager.tempPath + Path.DirectorySeparatorChar + downloadingSong.name + "." + downloadingSong.format, musicViewer.mediaPath + Path.DirectorySeparatorChar + downloadingSong.name + "." + downloadingSong.format );
			}

			currentDownloadPercentage = "";
			showSongInformation = false;
			downloading = false;
		}
	}	
	
	
	void DownloadProgressCallback ( object sender, DownloadProgressChangedEventArgs arg )
	{
	
		currentDownloadPercentage = " - " + arg.ProgressPercentage.ToString () + "% Complete";
	}
}