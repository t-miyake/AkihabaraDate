using System;
using System.Timers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Plugin.MediaManager;

namespace AkihabaraDate
{
    public class Model
    {
        private readonly List<SpotList> _spotList = new List<SpotList>
        {
            new SpotList{SpotName = "ダミーセリフ",Latitude = 0,Longitude = 0,VoiceName = "音声ファイル名.mp3"},
            new SpotList{SpotName = "秋葉原駅電気街口を出たところ",Latitude = 35.698466,Longitude = 139.773114,VoiceName = "音声ファイル名.mp3"},
            new SpotList{SpotName = "AKIHABARA_UDX",Latitude = 35.700525,Longitude = 139.772508,VoiceName = "音声ファイル名.mp3"},
            new SpotList{SpotName = "神田明神",Latitude = 35.701922,Longitude = 139.767846,VoiceName = "音声ファイル名.mp3"},
        };

        private double Latitude { get; set; }
        private double Longitude { get; set; }
        private int NearestPlaceNo { get; set; }
        private readonly Timer _timer = new Timer(2000);

        /// <summary>
        /// デートを開始する。
        /// </summary>
        public void RunDates()
        {
            _timer.Stop();

            var lastVoiceNo = 0;
            _timer.Elapsed += (sender, e) =>
            {
                Initialize();

                if (NearestPlaceNo == 0 || lastVoiceNo == NearestPlaceNo) return;
                PlayVoice(_spotList[NearestPlaceNo].VoiceName);
                lastVoiceNo = NearestPlaceNo;
            };

            _timer.Start();
        }

        /// <summary>
        /// デートを止める。
        /// </summary>
        public void StopDates()
        {
            _timer.Stop();
        }

        /// <summary>
        /// ユーザの現在地に合わせた音声を再生する
        /// </summary>
        public void ToggleTalk()
        {
            Initialize();
            PlayVoice(_spotList[NearestPlaceNo].VoiceName);
        }

        /// <summary>
        /// ユーザの現在地を取得し、一番近い音声再生スポットの場所を返す。
        /// </summary>
        public void Initialize()
        {
            GetPosition();
            NearestPlaceNo = GetNearestPlaceNo();
        }

        /// <summary>
        /// ユーザから一番近い場所までの距離を計算する。
        /// </summary>
        /// <returns>ユーザから一番近い場所のIndex</returns>
        private int GetNearestPlaceNo()
        {
            var distance = new List<int>();

            foreach (var spot in _spotList)
            {
                distance.Add(CalcDistance(Latitude, Longitude, spot.Latitude, spot.Longitude));
            }

            //一番近い場所に対して100m以内だったら、その場所のIndexを返す。そうでなければとりあえず0を返す。
            return distance.Min() <= 100 ? distance.IndexOf(distance.Min()) : 0;
        }


        /// <summary>
        /// 声を再生する。
        /// </summary>
        private async void PlayVoice(string voiceName)
        {
            CrossMediaManager.Current.MediaQueue.Clear();
            await CrossMediaManager.Current.Play("https://音声ファイルを置いたURL/" + voiceName);
        }

        /// <summary>
        /// 声を止める
        /// </summary>
        public void StopVoice()
        {
            CrossMediaManager.Current.Stop();
        }

        /// <summary>
        /// 緯度経度を2つ渡すと、その間の距離を返す。
        /// </summary>
        /// <param name="lat1">緯度1</param>
        /// <param name="long1">経度1</param>
        /// <param name="lat2">緯度2</param>
        /// <param name="long2">経度2</param>
        /// <returns>間の距離</returns>
        private static int CalcDistance(double lat1, double long1, double lat2, double long2)
        {
            var latAvg = Deg2Rad(lat1 + ((lat2 - lat1) / 2));
            var latDifference = Deg2Rad(lat1 - lat2);
            var lonDifference = Deg2Rad(long1 - long2);
            var curRadiusTemp = 1 - 0.00669438 * Math.Pow(Math.Sin(latAvg), 2);
            var meridianCurvatureRadius = 6335439.327 / Math.Sqrt(Math.Pow(curRadiusTemp, 3));
            var primeVerticalCircleCurvatureRadius = 6378137 / Math.Sqrt(curRadiusTemp);

            var distance = Math.Pow(meridianCurvatureRadius * latDifference, 2)
                           + Math.Pow(primeVerticalCircleCurvatureRadius * Math.Cos(latAvg) * lonDifference, 2);
            distance = Math.Sqrt(distance);

            return (int)Math.Round(distance);
        }

        /// <summary>
        /// ラジアン変換。
        /// </summary>
        /// <param name="deg">角度(度数法)</param>
        /// <returns>ラジアン</returns>
        private static double Deg2Rad(double deg)
        {
            return (deg / 180) * Math.PI;
        }

        /// <summary>
        /// ユーザの現在位置を取得し、緯度経度を格納する。
        /// </summary>
        private async void GetPosition()
        {
            var position = await GetGeolocate();
            Latitude = position.Latitude;
            Longitude = position.Longitude;
        }

        /// <summary>
        /// ユーザの現在位置を取得する。
        /// </summary>
        /// <returns>ユーザの現在位置</returns>
        private static async Task<Position> GetGeolocate()
        {
            var locator = CrossGeolocator.Current;

            //1. 50m
            locator.DesiredAccuracy = 50;
            return await locator.GetPositionAsync(TimeSpan.FromSeconds(0.5));
        }

    }
}