using Gameplay.Core;

namespace Gameplay.Interfaces
{
    /// <summary>
    /// Interface cho các component cần biết về trạng thái game phase
    /// </summary>
    public interface IGamePhaseAware
    {
        /// <summary>
        /// Được gọi khi game chuyển sang prepare phase
        /// </summary>
        void OnPreparePhaseStarted();

        /// <summary>
        /// Được gọi khi game chuyển sang battle phase
        /// </summary>
        void OnBattlePhaseStarted();

        /// <summary>
        /// Được gọi khi game phase thay đổi
        /// </summary>
        /// <param name="newPhase">Phase mới</param>
        void OnPhaseChanged(GamePhase newPhase);
    }
}
