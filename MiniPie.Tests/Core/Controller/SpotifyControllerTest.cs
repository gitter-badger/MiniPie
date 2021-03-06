﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using MiniPie.Core;
using MiniPie.Core.SpotifyLocal;
using MiniPie.Core.SpotifyNative;
using MiniPie.Core.SpotifyWeb;
using MiniPie.Core.SpotifyWeb.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using Track = MiniPie.Core.SpotifyLocal.Track;

namespace MiniPie.Tests.Core.Controller
{
    public class SpotifyControllerTest: IDisposable
    {
        private readonly SpotifyController _spotifyController;
        private readonly ISpotifyLocalApi _localApi;
        private readonly ILog _log;
        private readonly ISpotifyNativeApi _nativeApi;
        private readonly ISpotifyWebApi _spotifyWebApi;

        public SpotifyControllerTest()
        {
            _localApi = Substitute.For<ISpotifyLocalApi>();
            _spotifyWebApi = Substitute.For<ISpotifyWebApi>();
            _nativeApi = Substitute.For<ISpotifyNativeApi>();
            _log = Substitute.For<ILog>();

            _spotifyController = new SpotifyController(_log, _localApi, _spotifyWebApi, _nativeApi);
        }

        [Fact]
        public void ProcessTrackInfoTest()
        {
            bool trackChanged = false;
            bool trackTimerFiler = false;
            _spotifyController.TrackStatusChanged += (sender, args) =>
            {
                trackTimerFiler = true;
            };
            _spotifyController.TrackChanged += (sender, args) =>
            {
                trackChanged = true;
            };
            var status = new Status();
            status.track = new Track
            {
                track_resource = new Resource
                {
                    uri = "Awesome Track"
                }
            };
            _spotifyController.ProcessTrackInfo(status);
            Assert.True(trackChanged);
            
            _spotifyController.ProcessTrackInfo(status);
            Assert.True(trackTimerFiler);
        }

        [Fact]
        public void GetDelayForPlaybackUpdateTest()
        {
            double position1 = 12.43;
            double position2 = 139848934.3423240;
            Assert.Equal(_spotifyController.GetDelayForPlaybackUpdate(position1), 570);
            Assert.Equal(_spotifyController.GetDelayForPlaybackUpdate(position2), 657);
        }

        [Fact]
        public void GetStatusTest()
        {
            var status = new Status();
            _spotifyController.ProcessTrackInfo(status);
            Assert.Equal(status, _spotifyController.GetStatus());
        }

        [Fact]
        public async Task TestBackgroundChangeTrackerWorkCancelled()
        {
            _spotifyController.BackgroundDelay = 0;
            _spotifyController.SpotifyProcess = new Process();
            _localApi.SendLocalStatusRequest(true, true, CancellationToken.None, 0)
                .ThrowsForAnyArgs(new TaskCanceledException());
            await _spotifyController.BackgroundChangeTrackerWork();
            _log.Received(1).Info("Retrieving cancelled");
        }

        [Fact]
        public async Task TestBackgroundChangeTrackerWorkEmptyResult()
        {
            _spotifyController.BackgroundDelay = 0;
            _spotifyController.SpotifyProcess = new Process();
            _localApi.SendLocalStatusRequest(true, true, CancellationToken.None, 0)
                .ReturnsForAnyArgs(Task.FromResult<Status>(null));
            await _spotifyController.BackgroundChangeTrackerWork();
            _log.Received(1).Warn("Failed to retrieve track info. Track info is empty");
        }

        [Fact]
        public async Task TestBackgroundChangeTrackerWorkOauth()
        {
            _spotifyController.BackgroundDelay = 0;
            _spotifyController.SpotifyProcess = new Process();
            _localApi.SendLocalStatusRequest(true, true, CancellationToken.None, 0)
                .ReturnsForAnyArgs(Task.FromResult(new Status
                {
                    error = new Error
                    {
                        message = "OAuth updateToken blabla"
                    }
                }));
            await _spotifyController.BackgroundChangeTrackerWork();
            await _localApi.Received(1).RenewToken();
        }

        [Fact]
        public async Task TestBackgroundChangeTrackerWorkSpotifyError()
        {
            _spotifyController.BackgroundDelay = 0;
            _spotifyController.SpotifyProcess = new Process();
            _localApi.SendLocalStatusRequest(true, true, CancellationToken.None, 0)
                .ReturnsForAnyArgs(Task.FromResult(new Status
                {
                    error = new Error
                    {
                        message = "Failed hard"
                    }
                }));
            await _spotifyController.BackgroundChangeTrackerWork();
            _log.Received(1).WarnException("Failed to retrieve trackinfo: Spotify API error: Failed hard", Arg.Any<Exception>());
        }

        [Fact]
        public async Task TestBackgroundChangeTrackerWorkSuccessDifferentTrack()
        {
            _spotifyController.BackgroundDelay = 0;
            _spotifyController.SpotifyProcess = new Process();
            _localApi.SendLocalStatusRequest(true, true, CancellationToken.None, 0)
                .ReturnsForAnyArgs(Task.FromResult(new Status
                {
                    track = new Track
                    {
                        track_resource = new Resource
                        {
                           name = "TestName",
                           uri = "TestUri"
                        }
                    },
                    
                }));
            await
                Assert.RaisesAsync<EventArgs>(handler =>
                {
                    _spotifyController.TrackChanged += handler;
                },
                    handler =>
                    {
                        _spotifyController.TrackChanged -= handler;
                    },
                    () => _spotifyController.BackgroundChangeTrackerWork());
        }

        [Fact]
        public async Task TestPause()
        {
            _spotifyController.CurrentTrackInfo = new Status();
            _spotifyController.CurrentTrackInfo.playing = true;
            await _spotifyController.Pause();
            await _localApi.Received(1).Pause();

            _localApi.ClearReceivedCalls();
            _spotifyController.CurrentTrackInfo.playing = false;
            await _spotifyController.Pause();
            await _localApi.DidNotReceive().Pause();
        }

        [Fact]
        public async Task TestPlay()
        {
            _spotifyController.CurrentTrackInfo = new Status();
            _spotifyController.CurrentTrackInfo.playing = false;
            await _spotifyController.Play();
            await _localApi.Received(1).Resume();

            _localApi.ClearReceivedCalls();
            _spotifyController.CurrentTrackInfo.playing = true;
            await _spotifyController.Play();
            await _localApi.DidNotReceive().Resume();
        }

        [Fact]
        public async Task TestPlayPause()
        {
            _spotifyController.CurrentTrackInfo = new Status();
            _spotifyController.CurrentTrackInfo.playing = false;
            await _spotifyController.PausePlay();
            await _localApi.Received(1).Resume();
            await _localApi.DidNotReceive().Pause();

            _localApi.ClearReceivedCalls();
            _spotifyController.CurrentTrackInfo.playing = true;
            await _spotifyController.PausePlay();
            await _localApi.Received(1).Pause();
            await _localApi.DidNotReceive().Resume();
        }

        [Fact]
        public async Task TestNextTrack()
        {
            await _spotifyController.NextTrack();
            _nativeApi.Received(1).NextTrack();
        }

        [Fact]
        public async Task TestPreviousTrack()
        {
            await _spotifyController.PreviousTrack();
            _nativeApi.Received(1).PreviousTrack();
        }

        [Fact]
        public async Task TestVolumeUp()
        {
            await _spotifyController.VolumeUp();
            _nativeApi.Received(1).VolumeUp();
        }

        [Fact]
        public async Task TestVolumeDown()
        {
            await _spotifyController.VolumeDown();
            _nativeApi.Received(1).VolumeDown();
        }

        [Fact]
        public void TestOpenSpotify()
        {
            _spotifyController.OpenSpotify();
            _nativeApi.Received(1).OpenSpotify();
        }

        [Fact]
        public async Task IsUserLoggedOnSuccess()
        {
            _spotifyWebApi.GetProfile().Returns(Task.FromResult(new User()));
            Assert.True(await _spotifyController.IsUserLoggedIn());
        }

        [Fact]
        public async Task IsUserLoggedOnFail()
        {
            _spotifyWebApi.GetProfile().Throws(new HttpRequestException());
            Assert.False(await _spotifyController.IsUserLoggedIn());
        }

        [Fact]
        public void TestBuildLoginQuery()
        {
            var targetUri = new Uri("http://example.com");
            _spotifyWebApi.BuildLoginQuery().Returns(targetUri);
            var result = _spotifyController.BuildLoginQuery();
            Assert.Equal(targetUri, result);
        }

        [Fact]
        public void TestLogout()
        {
            _spotifyController.Logout();
            _spotifyWebApi.Received(1).Logout();
        }

        [Fact]
        public async Task TestGetPlaylists()
        {
            var playlists = new List<Playlist>();
            _spotifyWebApi.GetUserPlaylists().Returns(playlists);
            Assert.Equal(playlists, await _spotifyController.GetPlaylists());
        }

        [Fact]
        public async Task TestAddToPlaylist()
        {
            string playlistId = "playlistId";
            string trackUrl = "trackUrl";

            await _spotifyController.AddToPlaylist(playlistId, trackUrl);
            await _spotifyWebApi.Received(1).AddToPlaylist(playlistId, trackUrl);
        }

        [Fact]
        public async Task TestGetTrackInfo()
        {
            var trackIds = new[]
            {
                "Id"
            };

            var tracks = new List<MiniPie.Core.SpotifyWeb.Models.Track>();
            _spotifyWebApi.GetTrackInfo(trackIds).Returns(tracks);

            var result = await _spotifyController.GetTrackInfo(trackIds);
            await _spotifyWebApi.Received(1).GetTrackInfo(trackIds);
            Assert.Equal(tracks, result);
        }

        [Fact]
        public async Task TestAddToQueue()
        {
            var songUrls = new[]
            {
                "url1",
                "url2"
            };

            await _spotifyController.AddToQueue(songUrls);
            await _localApi.Received(1).Queue(songUrls[0]);
            await _localApi.Received(1).Queue(songUrls[1]);
        }

        [Fact]
        public async Task TestIsTrackSaved()
        {
            var trackIds = new[]
            {
                "Id"
            };

            var expected = new[]
            {
                false
            };

            _spotifyWebApi.IsTracksSaved(trackIds).Returns(expected);

            var result = await _spotifyController.IsTracksSaved(trackIds);
            await _spotifyWebApi.Received(1).IsTracksSaved(trackIds);
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task TestAddToMyMusic()
        {
            var trackIds = new[]
            {
                "Id"
            };

            await _spotifyController.AddToMyMusic(trackIds);
            await _spotifyWebApi.Received(1).AddToMyMusic(trackIds);
        }

        [Fact]
        public async Task TestRemoveFromMyMusic()
        {
            var trackIds = new[]
            {
                "Id"
            };

            await _spotifyController.RemoveFromMyMusic(trackIds);
            await _spotifyWebApi.Received(1).RemoveFromMyMusic(trackIds);
        }

        public void Dispose()
        {
            _spotifyController.Dispose();
        }
    }
}
