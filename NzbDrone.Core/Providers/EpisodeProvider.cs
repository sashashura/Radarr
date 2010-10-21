using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers
{
    public class EpisodeProvider : IEpisodeProvider
    {
        //TODO: Remove parsing of the series name, it should be done in series provider

        private readonly IRepository _sonicRepo;
        private readonly ISeriesProvider _series;
        private readonly ISeasonProvider _seasons;
        private readonly ITvDbProvider _tvDb;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        public EpisodeProvider(IRepository sonicRepo, ISeriesProvider seriesProvider, ISeasonProvider seasonProvider, ITvDbProvider tvDbProvider)
        {
            _sonicRepo = sonicRepo;
            _series = seriesProvider;
            _tvDb = tvDbProvider;
            _seasons = seasonProvider;
        }

        public Episode GetEpisode(long id)
        {
            return _sonicRepo.Single<Episode>(e => e.EpisodeId == id);
        }

        public IList<Episode> GetEpisodeBySeries(long seriesId)
        {
            return _sonicRepo.Find<Episode>(e => e.SeriesId == seriesId);
        }

        public String GetSabTitle(Episode episode)
        {
            var series = _series.GetSeries(episode.SeriesId);
            if (series == null) throw new ArgumentException("Unknown series. ID: " + episode.SeriesId);

            //TODO: This method should return a standard title for the sab episode.
            throw new NotImplementedException();
        }

        /// <summary>
        /// Comprehensive check on whether or not this episode is needed.
        /// </summary>
        /// <param name="episode">Episode that needs to be checked</param>
        /// <returns></returns>
        public bool IsNeeded(EpisodeModel episode)
        {
            throw new NotImplementedException();
        }

        public void RefreshEpisodeInfo(int seriesId)
        {
            Logger.Info("Starting episode info refresh for series:{0}", seriesId);
            int successCount = 0;
            int failCount = 0;
            var targetSeries = _tvDb.GetSeries(seriesId, true);

            var updateList = new List<Episode>();
            var newList = new List<Episode>();

            Logger.Debug("Updating season info for series:{0}", seriesId);
            targetSeries.Episodes.Select(e => new { e.SeasonId, e.SeasonNumber })
                .Distinct().ToList()
                .ForEach(s => _seasons.EnsureSeason(seriesId, s.SeasonId, s.SeasonNumber));

            foreach (var episode in targetSeries.Episodes)
            {
                try
                {
                    Logger.Debug("Updating info for series:{0} - episode:{1}", seriesId, episode.Id);
                    var newEpisode = new Episode()
                      {
                          AirDate = episode.FirstAired,
                          EpisodeId = episode.Id,
                          EpisodeNumber = episode.EpisodeNumber,
                          Language = episode.Language.Abbriviation,
                          Overview = episode.Overview,
                          SeasonId = episode.SeasonId,
                          SeasonNumber = episode.SeasonNumber,
                          SeriesId = seriesId,
                          Title = episode.EpisodeName
                      };

                    if (_sonicRepo.Exists<Episode>(e => e.EpisodeId == newEpisode.EpisodeId))
                    {
                        updateList.Add(newEpisode);
                    }
                    else
                    {
                        newList.Add(newEpisode);
                    }

                    successCount++;
                }
                catch (Exception e)
                {
                    Logger.FatalException(String.Format("An error has occurred while updating episode info for series {0}", seriesId), e);
                    failCount++;
                }
            }

            _sonicRepo.AddMany(newList);
            _sonicRepo.UpdateMany(updateList);

            Logger.Info("Finished episode refresh for series:{0}. Success:{1} - Fail:{2} ", seriesId, successCount, failCount);
        }
    }
}