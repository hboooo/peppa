using System;
using System.IO;
using System.Media;
using System.Windows.Media;

namespace Peppa.Utils
{
    public class MusicPlayer
    {
        private MediaPlayer _player = null;


        public void Play()
        {
            if (_player == null)
            {
                try
                {
                    _player = new MediaPlayer();
                    _player.MediaEnded += _player_MediaEnded;
                    _player.Open(new Uri(Path.Combine(System.Environment.CurrentDirectory, @"Resources\pushu_naxiehuaer.mp3")));
                }
                catch (Exception ex)
                {
                    PeppaUtils.Debug(ex);
                }
            }
            try
            {
                _player?.Play();
            }
            catch (Exception err)
            {
                PeppaUtils.Debug(err);
            }
        }

        private void _player_MediaEnded(object sender, EventArgs e)
        {
            try
            {
                _player.Position = new TimeSpan(0);
            }
            catch (Exception err)
            {
                PeppaUtils.Debug(err);
            }
        }

        public void Stop()
        {
            try
            {
                _player?.Stop();
            }
            catch { }
        }

        public void Pause()
        {
            try
            {
                _player?.Pause();
            }
            catch { }
        }

        public static void MessageAlarm()
        {
            try
            {
                SoundPlayer player = new SoundPlayer();
                player.SoundLocation = Path.Combine(System.Environment.CurrentDirectory, @"Resources\ir_end.wav");
                player.Load();
                player.Play();
            }
            catch (Exception ex)
            {
                PeppaUtils.Debug(ex);
            }
        }
    }
}
