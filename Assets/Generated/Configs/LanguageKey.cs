using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnitySystemFramework.Localization
{
    /// <summary>
    /// A list that is generated from a ScriptableObject that implements IGenerateConfig.
    /// </summary>
    [Serializable]
    public struct LanguageKey : IEquatable<LanguageKey>
    {
        private LanguageKey(string key)
        {
            Key = key;
        }
        
        /// <summary>
        /// The key for this entry.
        /// </summary>
        [SerializeField]
        public string Key;
        
        #region Functions
        
        /// <summary>
        /// Gets the value of an InputKey using the field name. (Key)
        /// </summary>
        public static object Get(string key)
        {
            if(key == null)
                return null;
            _valueLookup.TryGetValue(key, out var value);
            return value;
        }
        
        /// <summary>
        /// Determines if the key provided is a valid entry.
        /// </summary>
        public static bool IsValid(string key)
        {
            if(key == null)
                return false;
            return _valueLookup.ContainsKey(key);
        }
        
        /// <summary>
        /// Determines if the value of this entry is the type specified.
        /// </summary>
        public bool Is<T>()
        {
            return Get(Key) is T;
        }
        
        
        /// <summary>
        /// Determines if this LanguageKey is valid.
        /// </summary>
        public bool IsValid()
        {
            return IsValid(Key);
        }
        
        #endregion // Functions
        
        #region Entries
        
        /// <summary>
        /// A default invalid key.
        /// </summary>
        public static readonly LanguageKey Invalid = default;
        
        /// <summary>
        /// Returns a new list with all of the entries from LanguageKey.
        /// </summary>
        public static readonly IList<KeyValuePair<string, object>> All;
        
        public static readonly LanguageKey Actions_Attack = new LanguageKey(nameof(Actions_Attack));
        
        public static readonly LanguageKey Actions_Attack_Description = new LanguageKey(nameof(Actions_Attack_Description));
        
        public static readonly LanguageKey Actions_EnterGrow = new LanguageKey(nameof(Actions_EnterGrow));
        
        public static readonly LanguageKey Actions_EnterGrow_Description = new LanguageKey(nameof(Actions_EnterGrow_Description));
        
        public static readonly LanguageKey Actions_ExitGrow = new LanguageKey(nameof(Actions_ExitGrow));
        
        public static readonly LanguageKey Actions_ExitGrow_Description = new LanguageKey(nameof(Actions_ExitGrow_Description));
        
        public static readonly LanguageKey Actions_GrowBrain = new LanguageKey(nameof(Actions_GrowBrain));
        
        public static readonly LanguageKey Actions_GrowBrain_Description = new LanguageKey(nameof(Actions_GrowBrain_Description));
        
        public static readonly LanguageKey Actions_GrowEye = new LanguageKey(nameof(Actions_GrowEye));
        
        public static readonly LanguageKey Actions_GrowEye_Description = new LanguageKey(nameof(Actions_GrowEye_Description));
        
        public static readonly LanguageKey Actions_GrowGland = new LanguageKey(nameof(Actions_GrowGland));
        
        public static readonly LanguageKey Actions_GrowGland_Description = new LanguageKey(nameof(Actions_GrowGland_Description));
        
        public static readonly LanguageKey Actions_GrowLiver = new LanguageKey(nameof(Actions_GrowLiver));
        
        public static readonly LanguageKey Actions_GrowLiver_Description = new LanguageKey(nameof(Actions_GrowLiver_Description));
        
        public static readonly LanguageKey Actions_GrowLung = new LanguageKey(nameof(Actions_GrowLung));
        
        public static readonly LanguageKey Actions_GrowLung_Description = new LanguageKey(nameof(Actions_GrowLung_Description));
        
        public static readonly LanguageKey Actions_GrowMortar = new LanguageKey(nameof(Actions_GrowMortar));
        
        public static readonly LanguageKey Actions_GrowMortar_Description = new LanguageKey(nameof(Actions_GrowMortar_Description));
        
        public static readonly LanguageKey Actions_GrowMouth = new LanguageKey(nameof(Actions_GrowMouth));
        
        public static readonly LanguageKey Actions_GrowMouth_Description = new LanguageKey(nameof(Actions_GrowMouth_Description));
        
        public static readonly LanguageKey Actions_GrowTentacle = new LanguageKey(nameof(Actions_GrowTentacle));
        
        public static readonly LanguageKey Actions_GrowTentacle_Description = new LanguageKey(nameof(Actions_GrowTentacle_Description));
        
        public static readonly LanguageKey Actions_GrowTile = new LanguageKey(nameof(Actions_GrowTile));
        
        public static readonly LanguageKey Actions_GrowTile_Description = new LanguageKey(nameof(Actions_GrowTile_Description));
        
        public static readonly LanguageKey Actions_GrowTurret = new LanguageKey(nameof(Actions_GrowTurret));
        
        public static readonly LanguageKey Actions_GrowTurret_Description = new LanguageKey(nameof(Actions_GrowTurret_Description));
        
        public static readonly LanguageKey Actions_Heal = new LanguageKey(nameof(Actions_Heal));
        
        public static readonly LanguageKey Actions_Heal_Description = new LanguageKey(nameof(Actions_Heal_Description));
        
        public static readonly LanguageKey Actions_Move = new LanguageKey(nameof(Actions_Move));
        
        public static readonly LanguageKey Actions_Move_Description = new LanguageKey(nameof(Actions_Move_Description));
        
        public static readonly LanguageKey Actions_Stop = new LanguageKey(nameof(Actions_Stop));
        
        public static readonly LanguageKey Actions_Stop_Description = new LanguageKey(nameof(Actions_Stop_Description));
        
        public static readonly LanguageKey Actions_Submerge = new LanguageKey(nameof(Actions_Submerge));
        
        public static readonly LanguageKey Actions_Submerge_Description = new LanguageKey(nameof(Actions_Submerge_Description));
        
        public static readonly LanguageKey Actions_Surface = new LanguageKey(nameof(Actions_Surface));
        
        public static readonly LanguageKey Actions_Surface_Description = new LanguageKey(nameof(Actions_Surface_Description));
        
        public static readonly LanguageKey ActionsPanel_Title = new LanguageKey(nameof(ActionsPanel_Title));
        
        public static readonly LanguageKey AreanaMode_BuildTime = new LanguageKey(nameof(AreanaMode_BuildTime));
        
        public static readonly LanguageKey ArenaMode_Back = new LanguageKey(nameof(ArenaMode_Back));
        
        public static readonly LanguageKey ArenaMode_Difficulty = new LanguageKey(nameof(ArenaMode_Difficulty));
        
        public static readonly LanguageKey ArenaMode_Start = new LanguageKey(nameof(ArenaMode_Start));
        
        public static readonly LanguageKey BuildPanel_Tier = new LanguageKey(nameof(BuildPanel_Tier));
        
        public static readonly LanguageKey BuildPanel_Title = new LanguageKey(nameof(BuildPanel_Title));
        
        public static readonly LanguageKey Campaign_Back = new LanguageKey(nameof(Campaign_Back));
        
        public static readonly LanguageKey Campaign_Play = new LanguageKey(nameof(Campaign_Play));
        
        public static readonly LanguageKey Controls_AttackEat_Description = new LanguageKey(nameof(Controls_AttackEat_Description));
        
        public static readonly LanguageKey Controls_AttackEat_Label = new LanguageKey(nameof(Controls_AttackEat_Label));
        
        public static readonly LanguageKey Controls_GrowBrain_Description = new LanguageKey(nameof(Controls_GrowBrain_Description));
        
        public static readonly LanguageKey Controls_GrowBrain_Label = new LanguageKey(nameof(Controls_GrowBrain_Label));
        
        public static readonly LanguageKey Controls_GrowEye_Description = new LanguageKey(nameof(Controls_GrowEye_Description));
        
        public static readonly LanguageKey Controls_GrowEye_Label = new LanguageKey(nameof(Controls_GrowEye_Label));
        
        public static readonly LanguageKey Controls_GrowGland_Description = new LanguageKey(nameof(Controls_GrowGland_Description));
        
        public static readonly LanguageKey Controls_GrowGland_Label = new LanguageKey(nameof(Controls_GrowGland_Label));
        
        public static readonly LanguageKey Controls_GrowLiver_Description = new LanguageKey(nameof(Controls_GrowLiver_Description));
        
        public static readonly LanguageKey Controls_GrowLiver_Label = new LanguageKey(nameof(Controls_GrowLiver_Label));
        
        public static readonly LanguageKey Controls_GrowLung_Description = new LanguageKey(nameof(Controls_GrowLung_Description));
        
        public static readonly LanguageKey Controls_GrowLung_Label = new LanguageKey(nameof(Controls_GrowLung_Label));
        
        public static readonly LanguageKey Controls_GrowMortar_Description = new LanguageKey(nameof(Controls_GrowMortar_Description));
        
        public static readonly LanguageKey Controls_GrowMortar_Label = new LanguageKey(nameof(Controls_GrowMortar_Label));
        
        public static readonly LanguageKey Controls_GrowMouth_Description = new LanguageKey(nameof(Controls_GrowMouth_Description));
        
        public static readonly LanguageKey Controls_GrowMouth_Label = new LanguageKey(nameof(Controls_GrowMouth_Label));
        
        public static readonly LanguageKey Controls_GrowTentacle_Description = new LanguageKey(nameof(Controls_GrowTentacle_Description));
        
        public static readonly LanguageKey Controls_GrowTentacle_Label = new LanguageKey(nameof(Controls_GrowTentacle_Label));
        
        public static readonly LanguageKey Controls_GrowTiles_Description = new LanguageKey(nameof(Controls_GrowTiles_Description));
        
        public static readonly LanguageKey Controls_GrowTiles_Label = new LanguageKey(nameof(Controls_GrowTiles_Label));
        
        public static readonly LanguageKey Controls_GrowTurret_Description = new LanguageKey(nameof(Controls_GrowTurret_Description));
        
        public static readonly LanguageKey Controls_GrowTurret_Label = new LanguageKey(nameof(Controls_GrowTurret_Label));
        
        public static readonly LanguageKey Controls_Move_Description = new LanguageKey(nameof(Controls_Move_Description));
        
        public static readonly LanguageKey Controls_Move_Label = new LanguageKey(nameof(Controls_Move_Label));
        
        public static readonly LanguageKey Controls_Regen_Description = new LanguageKey(nameof(Controls_Regen_Description));
        
        public static readonly LanguageKey Controls_Regen_Label = new LanguageKey(nameof(Controls_Regen_Label));
        
        public static readonly LanguageKey Controls_Stop_Description = new LanguageKey(nameof(Controls_Stop_Description));
        
        public static readonly LanguageKey Controls_Stop_Label = new LanguageKey(nameof(Controls_Stop_Label));
        
        public static readonly LanguageKey HUD_Commands = new LanguageKey(nameof(HUD_Commands));
        
        public static readonly LanguageKey HUD_Grow = new LanguageKey(nameof(HUD_Grow));
        
        public static readonly LanguageKey HUD_NutrientBar = new LanguageKey(nameof(HUD_NutrientBar));
        
        public static readonly LanguageKey HUD_Objectives = new LanguageKey(nameof(HUD_Objectives));
        
        public static readonly LanguageKey HUD_SecondaryObjective = new LanguageKey(nameof(HUD_SecondaryObjective));
        
        public static readonly LanguageKey HUD_SecondaryObjectiveDescription = new LanguageKey(nameof(HUD_SecondaryObjectiveDescription));
        
        public static readonly LanguageKey HUD_SecondaryObjectiveFinished = new LanguageKey(nameof(HUD_SecondaryObjectiveFinished));
        
        public static readonly LanguageKey HUD_SkipTutorial = new LanguageKey(nameof(HUD_SkipTutorial));
        
        public static readonly LanguageKey Info_Plant_Desc = new LanguageKey(nameof(Info_Plant_Desc));
        
        public static readonly LanguageKey Info_Plant_Title = new LanguageKey(nameof(Info_Plant_Title));
        
        public static readonly LanguageKey Lobby_EnterText = new LanguageKey(nameof(Lobby_EnterText));
        
        public static readonly LanguageKey Lobby_LeaveLobby = new LanguageKey(nameof(Lobby_LeaveLobby));
        
        public static readonly LanguageKey Lobby_MapTitle = new LanguageKey(nameof(Lobby_MapTitle));
        
        public static readonly LanguageKey Lobby_NextMap = new LanguageKey(nameof(Lobby_NextMap));
        
        public static readonly LanguageKey Lobby_Ready = new LanguageKey(nameof(Lobby_Ready));
        
        public static readonly LanguageKey Lobby_Send = new LanguageKey(nameof(Lobby_Send));
        
        public static readonly LanguageKey MainMenu_ArenaMode = new LanguageKey(nameof(MainMenu_ArenaMode));
        
        public static readonly LanguageKey MainMenu_Back = new LanguageKey(nameof(MainMenu_Back));
        
        public static readonly LanguageKey MainMenu_Campaign = new LanguageKey(nameof(MainMenu_Campaign));
        
        public static readonly LanguageKey MainMenu_Credits = new LanguageKey(nameof(MainMenu_Credits));
        
        public static readonly LanguageKey MainMenu_ExitGame = new LanguageKey(nameof(MainMenu_ExitGame));
        
        public static readonly LanguageKey MainMenu_JoinNow = new LanguageKey(nameof(MainMenu_JoinNow));
        
        public static readonly LanguageKey MainMenu_Multiplayer = new LanguageKey(nameof(MainMenu_Multiplayer));
        
        public static readonly LanguageKey MainMenu_Options = new LanguageKey(nameof(MainMenu_Options));
        
        public static readonly LanguageKey MainMenu_SinglePlayer = new LanguageKey(nameof(MainMenu_SinglePlayer));
        
        public static readonly LanguageKey Maps_Butterfly_Desc = new LanguageKey(nameof(Maps_Butterfly_Desc));
        
        public static readonly LanguageKey Maps_Butterfly_Title = new LanguageKey(nameof(Maps_Butterfly_Title));
        
        public static readonly LanguageKey Maps_CaveOfWonders_Desc = new LanguageKey(nameof(Maps_CaveOfWonders_Desc));
        
        public static readonly LanguageKey Maps_CaveOfWonders_Title = new LanguageKey(nameof(Maps_CaveOfWonders_Title));
        
        public static readonly LanguageKey Maps_FourCorners_Desc = new LanguageKey(nameof(Maps_FourCorners_Desc));
        
        public static readonly LanguageKey Maps_FourCorners_Title = new LanguageKey(nameof(Maps_FourCorners_Title));
        
        public static readonly LanguageKey Maps_Level = new LanguageKey(nameof(Maps_Level));
        
        public static readonly LanguageKey Messages_CreatureBeingSlowedDownByEnemy = new LanguageKey(nameof(Messages_CreatureBeingSlowedDownByEnemy));
        
        public static readonly LanguageKey Messages_CreatureTooBigToMove = new LanguageKey(nameof(Messages_CreatureTooBigToMove));
        
        public static readonly LanguageKey Messages_InsufficientFunds = new LanguageKey(nameof(Messages_InsufficientFunds));
        
        public static readonly LanguageKey Messages_InsufficientFundsToSplit = new LanguageKey(nameof(Messages_InsufficientFundsToSplit));
        
        public static readonly LanguageKey Messages_ResourceDepleted = new LanguageKey(nameof(Messages_ResourceDepleted));
        
        public static readonly LanguageKey Multiplayer_Connect = new LanguageKey(nameof(Multiplayer_Connect));
        
        public static readonly LanguageKey Multiplayer_HostOrIP = new LanguageKey(nameof(Multiplayer_HostOrIP));
        
        public static readonly LanguageKey Multiplayer_MainMenu = new LanguageKey(nameof(Multiplayer_MainMenu));
        
        public static readonly LanguageKey Multiplayer_Port = new LanguageKey(nameof(Multiplayer_Port));
        
        public static readonly LanguageKey Multiplayer_QuickPlay = new LanguageKey(nameof(Multiplayer_QuickPlay));
        
        public static readonly LanguageKey Multiplayer_RankedPlay = new LanguageKey(nameof(Multiplayer_RankedPlay));
        
        public static readonly LanguageKey Multiplayer_Title = new LanguageKey(nameof(Multiplayer_Title));
        
        public static readonly LanguageKey Nutrients_Acid = new LanguageKey(nameof(Nutrients_Acid));
        
        public static readonly LanguageKey Nutrients_Rare = new LanguageKey(nameof(Nutrients_Rare));
        
        public static readonly LanguageKey Nutrients_Simple = new LanguageKey(nameof(Nutrients_Simple));
        
        public static readonly LanguageKey Options_Apply = new LanguageKey(nameof(Options_Apply));
        
        public static readonly LanguageKey Options_ControlsTitle = new LanguageKey(nameof(Options_ControlsTitle));
        
        public static readonly LanguageKey Options_Discard = new LanguageKey(nameof(Options_Discard));
        
        public static readonly LanguageKey Options_LanguagesTitle = new LanguageKey(nameof(Options_LanguagesTitle));
        
        public static readonly LanguageKey Options_OptionsTitle = new LanguageKey(nameof(Options_OptionsTitle));
        
        public static readonly LanguageKey Organs_Brain_Description = new LanguageKey(nameof(Organs_Brain_Description));
        
        public static readonly LanguageKey Organs_Brain_Footer = new LanguageKey(nameof(Organs_Brain_Footer));
        
        public static readonly LanguageKey Organs_Brain_Title = new LanguageKey(nameof(Organs_Brain_Title));
        
        public static readonly LanguageKey Organs_Eye_Description = new LanguageKey(nameof(Organs_Eye_Description));
        
        public static readonly LanguageKey Organs_Eye_Footer = new LanguageKey(nameof(Organs_Eye_Footer));
        
        public static readonly LanguageKey Organs_Eye_Title = new LanguageKey(nameof(Organs_Eye_Title));
        
        public static readonly LanguageKey Organs_Gland_Description = new LanguageKey(nameof(Organs_Gland_Description));
        
        public static readonly LanguageKey Organs_Gland_Footer = new LanguageKey(nameof(Organs_Gland_Footer));
        
        public static readonly LanguageKey Organs_Gland_Title = new LanguageKey(nameof(Organs_Gland_Title));
        
        public static readonly LanguageKey Organs_Heart_Description = new LanguageKey(nameof(Organs_Heart_Description));
        
        public static readonly LanguageKey Organs_Heart_Footer = new LanguageKey(nameof(Organs_Heart_Footer));
        
        public static readonly LanguageKey Organs_Heart_Title = new LanguageKey(nameof(Organs_Heart_Title));
        
        public static readonly LanguageKey Organs_Liver_Description = new LanguageKey(nameof(Organs_Liver_Description));
        
        public static readonly LanguageKey Organs_Liver_Footer = new LanguageKey(nameof(Organs_Liver_Footer));
        
        public static readonly LanguageKey Organs_Liver_Title = new LanguageKey(nameof(Organs_Liver_Title));
        
        public static readonly LanguageKey Organs_Lung_Description = new LanguageKey(nameof(Organs_Lung_Description));
        
        public static readonly LanguageKey Organs_Lung_Footer = new LanguageKey(nameof(Organs_Lung_Footer));
        
        public static readonly LanguageKey Organs_Lung_Title = new LanguageKey(nameof(Organs_Lung_Title));
        
        public static readonly LanguageKey Organs_Mortar_Description = new LanguageKey(nameof(Organs_Mortar_Description));
        
        public static readonly LanguageKey Organs_Mortar_Footer = new LanguageKey(nameof(Organs_Mortar_Footer));
        
        public static readonly LanguageKey Organs_Mortar_Title = new LanguageKey(nameof(Organs_Mortar_Title));
        
        public static readonly LanguageKey Organs_Mouth_Description = new LanguageKey(nameof(Organs_Mouth_Description));
        
        public static readonly LanguageKey Organs_Mouth_Footer = new LanguageKey(nameof(Organs_Mouth_Footer));
        
        public static readonly LanguageKey Organs_Mouth_Title = new LanguageKey(nameof(Organs_Mouth_Title));
        
        public static readonly LanguageKey Organs_Tentacle_Description = new LanguageKey(nameof(Organs_Tentacle_Description));
        
        public static readonly LanguageKey Organs_Tentacle_Footer = new LanguageKey(nameof(Organs_Tentacle_Footer));
        
        public static readonly LanguageKey Organs_Tentacle_Title = new LanguageKey(nameof(Organs_Tentacle_Title));
        
        public static readonly LanguageKey Organs_Tile_Description = new LanguageKey(nameof(Organs_Tile_Description));
        
        public static readonly LanguageKey Organs_Tile_Footer = new LanguageKey(nameof(Organs_Tile_Footer));
        
        public static readonly LanguageKey Organs_Tile_Title = new LanguageKey(nameof(Organs_Tile_Title));
        
        public static readonly LanguageKey Organs_Turret_Description = new LanguageKey(nameof(Organs_Turret_Description));
        
        public static readonly LanguageKey Organs_Turret_Footer = new LanguageKey(nameof(Organs_Turret_Footer));
        
        public static readonly LanguageKey Organs_Turret_Title = new LanguageKey(nameof(Organs_Turret_Title));
        
        public static readonly LanguageKey OverlayScreens_Continue = new LanguageKey(nameof(OverlayScreens_Continue));
        
        public static readonly LanguageKey OverlayScreens_Defeat = new LanguageKey(nameof(OverlayScreens_Defeat));
        
        public static readonly LanguageKey OverlayScreens_Disconnected = new LanguageKey(nameof(OverlayScreens_Disconnected));
        
        public static readonly LanguageKey OverlayScreens_LeaveFeedback = new LanguageKey(nameof(OverlayScreens_LeaveFeedback));
        
        public static readonly LanguageKey OverlayScreens_MainMenu = new LanguageKey(nameof(OverlayScreens_MainMenu));
        
        public static readonly LanguageKey OverlayScreens_NotUsed = new LanguageKey(nameof(OverlayScreens_NotUsed));
        
        public static readonly LanguageKey OverlayScreens_Observe = new LanguageKey(nameof(OverlayScreens_Observe));
        
        public static readonly LanguageKey OverlayScreens_Pause = new LanguageKey(nameof(OverlayScreens_Pause));
        
        public static readonly LanguageKey OverlayScreens_Restart = new LanguageKey(nameof(OverlayScreens_Restart));
        
        public static readonly LanguageKey OverlayScreens_Settings = new LanguageKey(nameof(OverlayScreens_Settings));
        
        public static readonly LanguageKey OverlayScreens_Win = new LanguageKey(nameof(OverlayScreens_Win));
        
        public static readonly LanguageKey Ranking_Bronze1 = new LanguageKey(nameof(Ranking_Bronze1));
        
        public static readonly LanguageKey Ranking_Bronze2 = new LanguageKey(nameof(Ranking_Bronze2));
        
        public static readonly LanguageKey Ranking_Bronze3 = new LanguageKey(nameof(Ranking_Bronze3));
        
        public static readonly LanguageKey Ranking_Gold1 = new LanguageKey(nameof(Ranking_Gold1));
        
        public static readonly LanguageKey Ranking_Gold2 = new LanguageKey(nameof(Ranking_Gold2));
        
        public static readonly LanguageKey Ranking_Gold3 = new LanguageKey(nameof(Ranking_Gold3));
        
        public static readonly LanguageKey Ranking_Platinum1 = new LanguageKey(nameof(Ranking_Platinum1));
        
        public static readonly LanguageKey Ranking_Platinum2 = new LanguageKey(nameof(Ranking_Platinum2));
        
        public static readonly LanguageKey Ranking_Platinum3 = new LanguageKey(nameof(Ranking_Platinum3));
        
        public static readonly LanguageKey Ranking_Silver1 = new LanguageKey(nameof(Ranking_Silver1));
        
        public static readonly LanguageKey Ranking_Silver2 = new LanguageKey(nameof(Ranking_Silver2));
        
        public static readonly LanguageKey Ranking_Silver3 = new LanguageKey(nameof(Ranking_Silver3));
        
        public static readonly LanguageKey Ranking_Unranked = new LanguageKey(nameof(Ranking_Unranked));
        
        #endregion // Entries
        
        #region Lookup
        
        private static readonly Dictionary<string, object> _valueLookup;
        
        static LanguageKey()
        {
            _valueLookup = new Dictionary<string, object>()
            {
                {nameof(Actions_Attack), "Actions/Attack"},
                {nameof(Actions_Attack_Description), "Actions/Attack/Description"},
                {nameof(Actions_EnterGrow), "Actions/EnterGrow"},
                {nameof(Actions_EnterGrow_Description), "Actions/EnterGrow/Description"},
                {nameof(Actions_ExitGrow), "Actions/ExitGrow"},
                {nameof(Actions_ExitGrow_Description), "Actions/ExitGrow/Description"},
                {nameof(Actions_GrowBrain), "Actions/GrowBrain"},
                {nameof(Actions_GrowBrain_Description), "Actions/GrowBrain/Description"},
                {nameof(Actions_GrowEye), "Actions/GrowEye"},
                {nameof(Actions_GrowEye_Description), "Actions/GrowEye/Description"},
                {nameof(Actions_GrowGland), "Actions/GrowGland"},
                {nameof(Actions_GrowGland_Description), "Actions/GrowGland/Description"},
                {nameof(Actions_GrowLiver), "Actions/GrowLiver"},
                {nameof(Actions_GrowLiver_Description), "Actions/GrowLiver/Description"},
                {nameof(Actions_GrowLung), "Actions/GrowLung"},
                {nameof(Actions_GrowLung_Description), "Actions/GrowLung/Description"},
                {nameof(Actions_GrowMortar), "Actions/GrowMortar"},
                {nameof(Actions_GrowMortar_Description), "Actions/GrowMortar/Description"},
                {nameof(Actions_GrowMouth), "Actions/GrowMouth"},
                {nameof(Actions_GrowMouth_Description), "Actions/GrowMouth/Description"},
                {nameof(Actions_GrowTentacle), "Actions/GrowTentacle"},
                {nameof(Actions_GrowTentacle_Description), "Actions/GrowTentacle/Description"},
                {nameof(Actions_GrowTile), "Actions/GrowTile"},
                {nameof(Actions_GrowTile_Description), "Actions/GrowTile/Description"},
                {nameof(Actions_GrowTurret), "Actions/GrowTurret"},
                {nameof(Actions_GrowTurret_Description), "Actions/GrowTurret/Description"},
                {nameof(Actions_Heal), "Actions/Heal"},
                {nameof(Actions_Heal_Description), "Actions/Heal/Description"},
                {nameof(Actions_Move), "Actions/Move"},
                {nameof(Actions_Move_Description), "Actions/Move/Description"},
                {nameof(Actions_Stop), "Actions/Stop"},
                {nameof(Actions_Stop_Description), "Actions/Stop/Description"},
                {nameof(Actions_Submerge), "Actions/Submerge"},
                {nameof(Actions_Submerge_Description), "Actions/Submerge/Description"},
                {nameof(Actions_Surface), "Actions/Surface"},
                {nameof(Actions_Surface_Description), "Actions/Surface/Description"},
                {nameof(ActionsPanel_Title), "ActionsPanel/Title"},
                {nameof(AreanaMode_BuildTime), "AreanaMode/BuildTime"},
                {nameof(ArenaMode_Back), "ArenaMode/Back"},
                {nameof(ArenaMode_Difficulty), "ArenaMode/Difficulty"},
                {nameof(ArenaMode_Start), "ArenaMode/Start"},
                {nameof(BuildPanel_Tier), "BuildPanel/Tier"},
                {nameof(BuildPanel_Title), "BuildPanel/Title"},
                {nameof(Campaign_Back), "Campaign/Back"},
                {nameof(Campaign_Play), "Campaign/Play"},
                {nameof(Controls_AttackEat_Description), "Controls/AttackEat/Description"},
                {nameof(Controls_AttackEat_Label), "Controls/AttackEat/Label"},
                {nameof(Controls_GrowBrain_Description), "Controls/GrowBrain/Description"},
                {nameof(Controls_GrowBrain_Label), "Controls/GrowBrain/Label"},
                {nameof(Controls_GrowEye_Description), "Controls/GrowEye/Description"},
                {nameof(Controls_GrowEye_Label), "Controls/GrowEye/Label"},
                {nameof(Controls_GrowGland_Description), "Controls/GrowGland/Description"},
                {nameof(Controls_GrowGland_Label), "Controls/GrowGland/Label"},
                {nameof(Controls_GrowLiver_Description), "Controls/GrowLiver/Description"},
                {nameof(Controls_GrowLiver_Label), "Controls/GrowLiver/Label"},
                {nameof(Controls_GrowLung_Description), "Controls/GrowLung/Description"},
                {nameof(Controls_GrowLung_Label), "Controls/GrowLung/Label"},
                {nameof(Controls_GrowMortar_Description), "Controls/GrowMortar/Description"},
                {nameof(Controls_GrowMortar_Label), "Controls/GrowMortar/Label"},
                {nameof(Controls_GrowMouth_Description), "Controls/GrowMouth/Description"},
                {nameof(Controls_GrowMouth_Label), "Controls/GrowMouth/Label"},
                {nameof(Controls_GrowTentacle_Description), "Controls/GrowTentacle/Description"},
                {nameof(Controls_GrowTentacle_Label), "Controls/GrowTentacle/Label"},
                {nameof(Controls_GrowTiles_Description), "Controls/GrowTiles/Description"},
                {nameof(Controls_GrowTiles_Label), "Controls/GrowTiles/Label"},
                {nameof(Controls_GrowTurret_Description), "Controls/GrowTurret/Description"},
                {nameof(Controls_GrowTurret_Label), "Controls/GrowTurret/Label"},
                {nameof(Controls_Move_Description), "Controls/Move/Description"},
                {nameof(Controls_Move_Label), "Controls/Move/Label"},
                {nameof(Controls_Regen_Description), "Controls/Regen/Description"},
                {nameof(Controls_Regen_Label), "Controls/Regen/Label"},
                {nameof(Controls_Stop_Description), "Controls/Stop/Description"},
                {nameof(Controls_Stop_Label), "Controls/Stop/Label"},
                {nameof(HUD_Commands), "HUD/Commands"},
                {nameof(HUD_Grow), "HUD/Grow"},
                {nameof(HUD_NutrientBar), "HUD/NutrientBar"},
                {nameof(HUD_Objectives), "HUD/Objectives"},
                {nameof(HUD_SecondaryObjective), "HUD/SecondaryObjective"},
                {nameof(HUD_SecondaryObjectiveDescription), "HUD/SecondaryObjectiveDescription"},
                {nameof(HUD_SecondaryObjectiveFinished), "HUD/SecondaryObjectiveFinished"},
                {nameof(HUD_SkipTutorial), "HUD/SkipTutorial"},
                {nameof(Info_Plant_Desc), "Info/Plant/Desc"},
                {nameof(Info_Plant_Title), "Info/Plant/Title"},
                {nameof(Lobby_EnterText), "Lobby/EnterText"},
                {nameof(Lobby_LeaveLobby), "Lobby/LeaveLobby"},
                {nameof(Lobby_MapTitle), "Lobby/MapTitle"},
                {nameof(Lobby_NextMap), "Lobby/NextMap"},
                {nameof(Lobby_Ready), "Lobby/Ready"},
                {nameof(Lobby_Send), "Lobby/Send"},
                {nameof(MainMenu_ArenaMode), "MainMenu/ArenaMode"},
                {nameof(MainMenu_Back), "MainMenu/Back"},
                {nameof(MainMenu_Campaign), "MainMenu/Campaign"},
                {nameof(MainMenu_Credits), "MainMenu/Credits"},
                {nameof(MainMenu_ExitGame), "MainMenu/ExitGame"},
                {nameof(MainMenu_JoinNow), "MainMenu/JoinNow"},
                {nameof(MainMenu_Multiplayer), "MainMenu/Multiplayer"},
                {nameof(MainMenu_Options), "MainMenu/Options"},
                {nameof(MainMenu_SinglePlayer), "MainMenu/SinglePlayer"},
                {nameof(Maps_Butterfly_Desc), "Maps/Butterfly/Desc"},
                {nameof(Maps_Butterfly_Title), "Maps/Butterfly/Title"},
                {nameof(Maps_CaveOfWonders_Desc), "Maps/CaveOfWonders/Desc"},
                {nameof(Maps_CaveOfWonders_Title), "Maps/CaveOfWonders/Title"},
                {nameof(Maps_FourCorners_Desc), "Maps/FourCorners/Desc"},
                {nameof(Maps_FourCorners_Title), "Maps/FourCorners/Title"},
                {nameof(Maps_Level), "Maps/Level"},
                {nameof(Messages_CreatureBeingSlowedDownByEnemy), "Messages/CreatureBeingSlowedDownByEnemy"},
                {nameof(Messages_CreatureTooBigToMove), "Messages/CreatureTooBigToMove"},
                {nameof(Messages_InsufficientFunds), "Messages/InsufficientFunds"},
                {nameof(Messages_InsufficientFundsToSplit), "Messages/InsufficientFundsToSplit"},
                {nameof(Messages_ResourceDepleted), "Messages/ResourceDepleted"},
                {nameof(Multiplayer_Connect), "Multiplayer/Connect"},
                {nameof(Multiplayer_HostOrIP), "Multiplayer/HostOrIP"},
                {nameof(Multiplayer_MainMenu), "Multiplayer/MainMenu"},
                {nameof(Multiplayer_Port), "Multiplayer/Port"},
                {nameof(Multiplayer_QuickPlay), "Multiplayer/QuickPlay"},
                {nameof(Multiplayer_RankedPlay), "Multiplayer/RankedPlay"},
                {nameof(Multiplayer_Title), "Multiplayer/Title"},
                {nameof(Nutrients_Acid), "Nutrients/Acid"},
                {nameof(Nutrients_Rare), "Nutrients/Rare"},
                {nameof(Nutrients_Simple), "Nutrients/Simple"},
                {nameof(Options_Apply), "Options/Apply"},
                {nameof(Options_ControlsTitle), "Options/ControlsTitle"},
                {nameof(Options_Discard), "Options/Discard"},
                {nameof(Options_LanguagesTitle), "Options/LanguagesTitle"},
                {nameof(Options_OptionsTitle), "Options/OptionsTitle"},
                {nameof(Organs_Brain_Description), "Organs/Brain/Description"},
                {nameof(Organs_Brain_Footer), "Organs/Brain/Footer"},
                {nameof(Organs_Brain_Title), "Organs/Brain/Title"},
                {nameof(Organs_Eye_Description), "Organs/Eye/Description"},
                {nameof(Organs_Eye_Footer), "Organs/Eye/Footer"},
                {nameof(Organs_Eye_Title), "Organs/Eye/Title"},
                {nameof(Organs_Gland_Description), "Organs/Gland/Description"},
                {nameof(Organs_Gland_Footer), "Organs/Gland/Footer"},
                {nameof(Organs_Gland_Title), "Organs/Gland/Title"},
                {nameof(Organs_Heart_Description), "Organs/Heart/Description"},
                {nameof(Organs_Heart_Footer), "Organs/Heart/Footer"},
                {nameof(Organs_Heart_Title), "Organs/Heart/Title"},
                {nameof(Organs_Liver_Description), "Organs/Liver/Description"},
                {nameof(Organs_Liver_Footer), "Organs/Liver/Footer"},
                {nameof(Organs_Liver_Title), "Organs/Liver/Title"},
                {nameof(Organs_Lung_Description), "Organs/Lung/Description"},
                {nameof(Organs_Lung_Footer), "Organs/Lung/Footer"},
                {nameof(Organs_Lung_Title), "Organs/Lung/Title"},
                {nameof(Organs_Mortar_Description), "Organs/Mortar/Description"},
                {nameof(Organs_Mortar_Footer), "Organs/Mortar/Footer"},
                {nameof(Organs_Mortar_Title), "Organs/Mortar/Title"},
                {nameof(Organs_Mouth_Description), "Organs/Mouth/Description"},
                {nameof(Organs_Mouth_Footer), "Organs/Mouth/Footer"},
                {nameof(Organs_Mouth_Title), "Organs/Mouth/Title"},
                {nameof(Organs_Tentacle_Description), "Organs/Tentacle/Description"},
                {nameof(Organs_Tentacle_Footer), "Organs/Tentacle/Footer"},
                {nameof(Organs_Tentacle_Title), "Organs/Tentacle/Title"},
                {nameof(Organs_Tile_Description), "Organs/Tile/Description"},
                {nameof(Organs_Tile_Footer), "Organs/Tile/Footer"},
                {nameof(Organs_Tile_Title), "Organs/Tile/Title"},
                {nameof(Organs_Turret_Description), "Organs/Turret/Description"},
                {nameof(Organs_Turret_Footer), "Organs/Turret/Footer"},
                {nameof(Organs_Turret_Title), "Organs/Turret/Title"},
                {nameof(OverlayScreens_Continue), "OverlayScreens/Continue"},
                {nameof(OverlayScreens_Defeat), "OverlayScreens/Defeat"},
                {nameof(OverlayScreens_Disconnected), "OverlayScreens/Disconnected"},
                {nameof(OverlayScreens_LeaveFeedback), "OverlayScreens/LeaveFeedback"},
                {nameof(OverlayScreens_MainMenu), "OverlayScreens/MainMenu"},
                {nameof(OverlayScreens_NotUsed), "OverlayScreens/NotUsed"},
                {nameof(OverlayScreens_Observe), "OverlayScreens/Observe"},
                {nameof(OverlayScreens_Pause), "OverlayScreens/Pause"},
                {nameof(OverlayScreens_Restart), "OverlayScreens/Restart"},
                {nameof(OverlayScreens_Settings), "OverlayScreens/Settings"},
                {nameof(OverlayScreens_Win), "OverlayScreens/Win"},
                {nameof(Ranking_Bronze1), "Ranking/Bronze1"},
                {nameof(Ranking_Bronze2), "Ranking/Bronze2"},
                {nameof(Ranking_Bronze3), "Ranking/Bronze3"},
                {nameof(Ranking_Gold1), "Ranking/Gold1"},
                {nameof(Ranking_Gold2), "Ranking/Gold2"},
                {nameof(Ranking_Gold3), "Ranking/Gold3"},
                {nameof(Ranking_Platinum1), "Ranking/Platinum1"},
                {nameof(Ranking_Platinum2), "Ranking/Platinum2"},
                {nameof(Ranking_Platinum3), "Ranking/Platinum3"},
                {nameof(Ranking_Silver1), "Ranking/Silver1"},
                {nameof(Ranking_Silver2), "Ranking/Silver2"},
                {nameof(Ranking_Silver3), "Ranking/Silver3"},
                {nameof(Ranking_Unranked), "Ranking/Unranked"},
            };
            All = _valueLookup.ToList();
        }
        
        #endregion // Lookup
        
        #region Operators
        
        public static implicit operator LanguageKey(string key)
        {
            return new LanguageKey(key);
        }
        
        public static implicit operator bool(LanguageKey key)
        {
            return (bool) Get(key.Key);
        }
        
        public static implicit operator char(LanguageKey key)
        {
            return (char) Get(key.Key);
        }
        
        public static implicit operator byte(LanguageKey key)
        {
            return (byte) Get(key.Key);
        }
        
        public static implicit operator sbyte(LanguageKey key)
        {
            return (sbyte) Get(key.Key);
        }
        
        public static implicit operator short(LanguageKey key)
        {
            return (short) Get(key.Key);
        }
        
        public static implicit operator ushort(LanguageKey key)
        {
            return (ushort) Get(key.Key);
        }
        
        public static implicit operator int(LanguageKey key)
        {
            return (int) Get(key.Key);
        }
        
        public static implicit operator uint(LanguageKey key)
        {
            return (uint) Get(key.Key);
        }
        
        public static implicit operator float(LanguageKey key)
        {
            return (float) Get(key.Key);
        }
        
        public static implicit operator double(LanguageKey key)
        {
            return (double) Get(key.Key);
        }
        
        public static implicit operator long(LanguageKey key)
        {
            return (long) Get(key.Key);
        }
        
        public static implicit operator ulong(LanguageKey key)
        {
            return (ulong) Get(key.Key);
        }
        
        public static implicit operator decimal(LanguageKey key)
        {
            return (decimal) Get(key.Key);
        }
        
        public static implicit operator string(LanguageKey key)
        {
            return (string) Get(key.Key);
        }
        
        public bool Equals(LanguageKey other)
        {
            return Key == other.Key;
        }
        
        public override bool Equals(object obj)
        {
            return obj is LanguageKey other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            if(Key == null)
                return 0;
            var value = Get(Key);
            return (value != null ? value.GetHashCode() : 0);
        }
        
        public override string ToString()
        {
            if(Key == null)
                return default;
            return Get(Key) + "";
        }
        
        public static bool operator ==(LanguageKey a, LanguageKey b)
        {
            return a.Equals(b);
        }
        
        public static bool operator !=(LanguageKey a, LanguageKey b)
        {
            return !a.Equals(b);
        }
        
        #endregion // Operators
    }
}
