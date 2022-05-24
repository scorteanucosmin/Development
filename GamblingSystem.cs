using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Plugins;
using ProtoBuf;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Gambling System", "supreme", "1.0.9")]
    [Description("Private gambling system developed for Rust Academy")]
    public class GamblingSystem : RustPlugin
    {
        #region Class Fields

        [PluginReference] 
        private Plugin NexusUI;
        private static GamblingSystem _pluginInstance;
        private PluginConfig _pluginConfig;
        private PluginData _pluginData;
        
        private JackpotHandler _currentJackpot;

        private const string AdminPermission = "gamblingsystem.admin";

        #endregion

        #region Hooks

        private void Init()
        {
            _pluginInstance = this;
            LoadData();
        }

        private void OnServerInitialized()
        {
            _currentJackpot = new JackpotHandler();
            
            foreach (KeyValuePair<ulong, long> keyValuePair in _pluginData.CachedCoinFlipBets.ToList())
            {
                SetRp(keyValuePair.Key, CheckRp(keyValuePair.Key) + keyValuePair.Value);
                _pluginData.CachedCoinFlipBets.Remove(keyValuePair.Key);
            }
            
            foreach (KeyValuePair<ulong, long> keyValuePair in _pluginData.CachedJackpotBets.ToList())
            {
                SetRp(keyValuePair.Key, CheckRp(keyValuePair.Key) + keyValuePair.Value);
                _pluginData.CachedJackpotBets.Remove(keyValuePair.Key);
            }
            
            permission.RegisterPermission(AdminPermission, this);
        }

        private void Unload()
        {
            SaveData();
            _pluginInstance = null;
        }
        
        #endregion

        #region Helper Methods

        private void SetRp(ulong playerId, long rp)
        {
            NexusUI.Call("SetShopPoints", playerId, rp);
        }

        private long CheckRp(ulong playerId)
        {
            return NexusUI.Call<long>("GetShopPoints", playerId);
        }
        
        private BasePlayer FindPlayer(string arg)
        {
            return BasePlayer.activePlayerList.FirstOrDefault(p => p.displayName.Contains(arg, CompareOptions.OrdinalIgnoreCase) || p.UserIDString.Contains(arg)) 
                   ?? BasePlayer.sleepingPlayerList.FirstOrDefault(p => p.IsConnected && p.displayName.Contains(arg, CompareOptions.OrdinalIgnoreCase) || p.UserIDString.Contains(arg));
        }

        #endregion
        
        #region Configuration

        private class PluginConfig
        {
            [DefaultValue(14400f)]
            [JsonProperty(PropertyName = "Jackpot end frequency (seconds)")]
            public float JackpotEndFrequency { get; set; }
            
            [DefaultValue(7200f)]
            [JsonProperty(PropertyName = "Jackpot frequency (seconds)")]
            public float JackpotFrequency { get; set; }

            [DefaultValue(100)]
            [JsonProperty(PropertyName = "Minimum jackpot entry")]
            public double MinimumJackpotEntry { get; set; }
            
            [DefaultValue(1000)]
            [JsonProperty(PropertyName = "Maximum jackpot entry")]
            public double MaximumJackpotEntry { get; set; }
            
            [DefaultValue(120f)]
            [JsonProperty(PropertyName = "Coin flip request duration (seconds)")]
            public float CoinFlipRequestDuration { get; set; }
        }
        
        protected override void LoadDefaultConfig()
        {
            PrintWarning("Loading Default Config");
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            Config.Settings.DefaultValueHandling = DefaultValueHandling.Populate;
            _pluginConfig = AdditionalConfig(Config.ReadObject<PluginConfig>());
            Config.WriteObject(_pluginConfig);
        }

        private PluginConfig AdditionalConfig(PluginConfig pluginConfig)
        {
            return pluginConfig;
        }

        #endregion
        
        #region Data

        private void SaveData()
        {
            if (_pluginData == null)
            {
                return;
            }
            
            ProtoStorage.Save(_pluginData, Name);
        }

        private void LoadData()
        {
            _pluginData = ProtoStorage.Load<PluginData>(Name) ?? new PluginData();
        }

        [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
        private class PluginData
        {
            public List<WinnerData> Winners { get; set; } = new List<WinnerData>();
            public readonly Hash<ulong, long> CachedCoinFlipBets = new Hash<ulong, long>();
            public readonly Hash<ulong, long> CachedJackpotBets = new Hash<ulong, long>();
        }
        
        [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
        private class WinnerData
        {
            public string Name { get; set; }
            public double AmountWon { get; set; }
            public double Chance { get; set; }

            public WinnerData(BasePlayer player, double amountWon, double chance)
            {
                Name = player.displayName;
                AmountWon = amountWon;
                Chance = chance;
            }
        }

        #endregion
        
        #region Language
        
        private class LangKeys
        {
            public const string InsufficientBalance = nameof(InsufficientBalance);
            public const string InsufficientBalanceBet = nameof(InsufficientBalanceBet);
            public const string RaffleStart = nameof(RaffleStart);
            public const string RaffleEnd = nameof(RaffleEnd);
            public const string RaffleEndNoWinner = nameof(RaffleEndNoWinner);
            public const string RaffleWon = nameof(RaffleWon);
            public const string PlacedBet = nameof(PlacedBet);
            public const string PlacedBets = nameof(PlacedBets);
            public const string PlacedMultipleBets = nameof(PlacedMultipleBets);
            public const string JackPotEnding = nameof(JackPotEnding);
            public const string JackPotInvalidSyntax = nameof(JackPotInvalidSyntax);
            public const string JackPotMinimumEntry = nameof(JackPotMinimumEntry);
            public const string JackPotMaximumEntry = nameof(JackPotMaximumEntry);
            
            public const string CoinFlipMissingArgs = nameof(CoinFlipMissingArgs);
            public const string CoinFlipInvalidSyntax = nameof(CoinFlipInvalidSyntax);
            public const string NoPendingRequests = nameof(NoPendingRequests);
            public const string AlreadyPendingRequests = nameof(AlreadyPendingRequests);
            public const string AlreadySentRequest = nameof(AlreadySentRequest);
            public const string PendingRequestAccept = nameof(PendingRequestAccept);
            public const string PendingRequestDeny = nameof(PendingRequestDeny);
            public const string MissingPlayer = nameof(MissingPlayer);
            public const string CoinFlipYourself = nameof(CoinFlipYourself);
            public const string CoinFlipAlreadyIn = nameof(CoinFlipAlreadyIn);
            public const string CoinFlipPlayerAlreadyIn = nameof(CoinFlipPlayerAlreadyIn);
            public const string CoinFlipRequestSent = nameof(CoinFlipRequestSent);
            public const string CoinFlipRequestReceived = nameof(CoinFlipRequestReceived);
            public const string CoinFlipStarting = nameof(CoinFlipStarting);
            public const string CoinFlipWon = nameof(CoinFlipWon);
        }

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                [LangKeys.InsufficientBalance] = "Insufficient balance!",
                [LangKeys.RaffleStart] = "A raffle just started!\nType /jackpot enter amount to place a bet!",
                [LangKeys.RaffleEnd] = "The raffle just ended and the winner was picked! {0} won {1} RP with a chance of {2}%\nUse /jackpot enter amount (RP) to enter the jackpot!",
                [LangKeys.RaffleEndNoWinner] = "The raffle just ended without a winner!\nUse /jackpot enter amount (RP) to enter the jackpot!",
                [LangKeys.RaffleWon] = "You have won {0} RP",
                [LangKeys.PlacedBet] = "You just bet {0} RP, you have a chance of winning of {1}%",
                [LangKeys.PlacedBets] = "{0} Player(s) have put in the pot ({1} RP)!",
                [LangKeys.PlacedMultipleBets] = "A player added another bet in the pot, from {0} RP to {1} RP!",
                [LangKeys.JackPotEnding] = "The jackpot will end in: {0}\nThe pot value is: {1} RP",
                [LangKeys.JackPotInvalidSyntax] = "Invalid syntax: Use /jackpot enter amount (RP)",
                [LangKeys.JackPotMinimumEntry] = "Minimum entry is {0} RP",
                [LangKeys.JackPotMaximumEntry] = "Maximum entry is {0} RP",
                
                [LangKeys.InsufficientBalanceBet] = "You cannot bet {0} RP, you only have {1} RP",
                [LangKeys.CoinFlipMissingArgs] = "Use /cf amount playerName/playerId\n/cf accept\n/cf deny",
                [LangKeys.NoPendingRequests] = "You do not have any pending requests!",
                [LangKeys.AlreadyPendingRequests] = "You already have a pending request!",
                [LangKeys.AlreadySentRequest] = "You already have sent a request!",
                [LangKeys.PendingRequestAccept] = "You have accepted a pending request!",
                [LangKeys.PendingRequestDeny] = "You have denied a pending request!",
                [LangKeys.CoinFlipInvalidSyntax] = "Invalid Syntax: Use /cf amount playerName/playerId",
                [LangKeys.MissingPlayer] = "The player you are searching for cannot be found!",
                [LangKeys.CoinFlipYourself] = "You cannot flip a coin yourself!",
                [LangKeys.CoinFlipAlreadyIn] = "You are already in a coin flip!",
                [LangKeys.CoinFlipPlayerAlreadyIn] = "{0} is already in a coin flip!",
                [LangKeys.CoinFlipRequestSent] = "You have sent a coin flip request to {0} for {1} RP ({2} RP each)",
                [LangKeys.CoinFlipRequestReceived] = "You have received a coin flip request from {0} for {1} RP ({2} RP each)\nUse /cf accept to continue",
                [LangKeys.CoinFlipStarting] = "Coin flip for {0} RP between {1} and {2} is now commencing!\nCoin flip will begin in 5...",
                [LangKeys.CoinFlipWon] = "Player {0} has won this coin flip for {1} RP!",
                
            }, this);
        }
        
        private string Lang(string key, BasePlayer player = null, params object[] args)
        {
            try
            {
                return string.Format(lang.GetMessage(key, this, player?.UserIDString), args);
            }
            catch (Exception ex)
            {
                PrintError($"Lang Key '{key}' threw exception:\n{ex}");
                throw;
            }
        }

        #endregion

        #region Chat Commands

        [ChatCommand("cf")]
        private void CoinFlipCommand(BasePlayer player, string command, string[] args)
        {
            if (args.Length == 0)
            {
                player.ChatMessage(Lang(LangKeys.CoinFlipMissingArgs));
                return;
            }

            switch (args[0].ToLower())
            {
                case "accept":
                {
                    if (!HasPendingRequest(player))
                    {
                        player.ChatMessage(Lang(LangKeys.NoPendingRequests));
                        return;
                    }

                    int betAmount = GetBetAmount(player);
                    if (CheckRp(player.userID) < betAmount)
                    {
                        return;
                    }
                    
                    ulong sender = GetSender(player);
                    if (sender == 0)
                    {
                        return;
                    }

                    BasePlayer baseSender = BasePlayer.FindByID(sender);
                    new CoinFlipHandler(player, baseSender, betAmount * 2);
                    SetRp(player.userID, CheckRp(player.userID) - betAmount);
                    SetRp(baseSender.userID, CheckRp(baseSender.userID) - betAmount);
                    _pluginData.CachedCoinFlipBets[player.userID] = betAmount;
                    _pluginData.CachedCoinFlipBets[baseSender.userID] = betAmount;
                    SaveData();
                    _requests.Remove(sender);
                    player.ChatMessage(Lang(LangKeys.PendingRequestAccept));
                    return;
                }
                case "deny":
                {
                    if (!HasPendingRequest(player))
                    {
                        player.ChatMessage(Lang(LangKeys.NoPendingRequests));
                        return;
                    }

                    ulong sender = GetSender(player);
                    if (sender == 0)
                    {
                        return;
                    }
                    
                    _requests.Remove(sender);
                    player.ChatMessage(Lang(LangKeys.PendingRequestDeny));
                    return;
                }
            }

            switch (args.Length)
            {
                case 1:
                {
                    player.ChatMessage(Lang(LangKeys.CoinFlipInvalidSyntax));
                    return;
                }
                case 2:
                {
                    BasePlayer receiver = FindPlayer(args[1]);
                    if (!receiver)
                    {
                        player.ChatMessage(Lang(LangKeys.MissingPlayer));
                        return;
                    }
                    
                    if (player == receiver)
                    {
                        player.ChatMessage(Lang(LangKeys.CoinFlipYourself));
                        return;
                    }

                    int amount;
                    if (int.TryParse(args[0], out amount))
                    {
                        if (!receiver)
                        {
                            player.ChatMessage(Lang(LangKeys.CoinFlipInvalidSyntax));
                            return;
                        }

                        int availableRp = Convert.ToInt32(CheckRp(player.userID));
                        if (availableRp < amount)
                        {
                            player.ChatMessage(Lang(LangKeys.InsufficientBalance));
                            return;
                        }

                        if (HasPendingRequest(player))
                        {
                            player.ChatMessage(Lang(LangKeys.AlreadyPendingRequests));
                            return;
                        }

                        if (_requests.ContainsKey(player.userID))
                        {
                            player.ChatMessage(Lang(LangKeys.AlreadySentRequest));
                            return;
                        }

                        if (IsInCoinFlip(player))
                        {
                            player.ChatMessage(Lang(LangKeys.CoinFlipAlreadyIn));
                            return;
                        }

                        if (IsInCoinFlip(receiver))
                        {
                            player.ChatMessage(Lang(LangKeys.CoinFlipPlayerAlreadyIn, null, receiver.displayName));
                            return;
                        }

                        SendRequest(player, receiver, amount);
                    }
                    return;
                }
            }
        }

        [ChatCommand("jackpot")]
        private void JackpotCommand(BasePlayer player, string command, string[] args)
        {
            if (args.Length == 0)
            {
                player.ChatMessage(_currentJackpot == null ? "There is no active jackpot!" : Lang(LangKeys.JackPotEnding, null, $"{_currentJackpot.RemainingTime - DateTime.UtcNow:hh\\:mm\\:ss}", _currentJackpot.Pot));
                return;
            }

            switch (args[0].ToLower())
            {
                case "enter":
                {
                    if (_currentJackpot == null)
                    {
                        player.ChatMessage("There is no active jackpot!");
                        return;
                    }
                    
                    if (args.Length < 2)
                    {
                        player.ChatMessage(Lang(LangKeys.JackPotInvalidSyntax));
                        return;
                    }
                    
                    int amount;
                    if (int.TryParse(args[1], out amount))
                    {
                        if (_pluginConfig.MinimumJackpotEntry > amount)
                        {
                            player.ChatMessage(Lang(LangKeys.JackPotMinimumEntry, null, _pluginConfig.MinimumJackpotEntry));
                            return;
                        }

                        if (_pluginConfig.MaximumJackpotEntry < amount)
                        {
                            player.ChatMessage(Lang(LangKeys.JackPotMaximumEntry, null, _pluginConfig.MaximumJackpotEntry));
                            return;
                        }
                        
                        long availableRp = CheckRp(player.userID);
                        if (availableRp < amount)
                        {
                            player.ChatMessage(Lang(LangKeys.InsufficientBalance));
                        }
                        else
                        {
                            _currentJackpot.AddBet(player, amount);
                        }
                    }
                    else
                    {
                        player.ChatMessage(Lang(LangKeys.JackPotInvalidSyntax));
                    }
                    return;
                }
                case "winners":
                {
                    if (args.Length < 2)
                    {
                        IEnumerable<WinnerData> winners = _pluginData.Winners.Skip(6 * 0).Take(6);
                        StringBuilder stringBuilder = new StringBuilder();
                        foreach (WinnerData winner in winners)
                        {
                            stringBuilder.Append($"Winner: {winner.Name}\nAmount: {winner.AmountWon}\nChance: {winner.Chance}");
                        }
                        
                        player.ChatMessage($"Page: 0\n{stringBuilder}");
                    }
                    else
                    {
                        int page;
                        if (int.TryParse(args[1], out page))
                        {
                            IEnumerable<WinnerData> winners = _pluginData.Winners.Skip(6 * page).Take(6);
                            StringBuilder stringBuilder = new StringBuilder();
                            foreach (WinnerData winner in winners)
                            {
                                stringBuilder.Append($"Winner: {winner.Name}\nAmount: {winner.AmountWon}\nChance: {winner.Chance}");
                            }
                        
                            player.ChatMessage($"Page: {page}\n{stringBuilder}");
                        }
                    }
                    return;
                }
                case "forcedraw":
                {
                    if (!permission.UserHasPermission(player.UserIDString, AdminPermission))
                    {
                        player.ChatMessage("No permission!");
                        return;
                    }
                    
                    _currentJackpot?.EndRaffle();
                    break;
                }
            }
        }

        #endregion

        #region Request Handler

        private readonly Hash<ulong, RequestHandler> _requests = new Hash<ulong, RequestHandler>();

        private void SendRequest(BasePlayer sender, BasePlayer receiver, int amount)
        {
            _requests[sender.userID] = new RequestHandler(sender.userID, receiver.userID, amount);
            timer.Once(_pluginConfig.CoinFlipRequestDuration, () =>
            {
                _requests.Remove(sender.userID);
            });
            sender.ChatMessage(Lang(LangKeys.CoinFlipRequestSent, null, receiver.displayName, amount * 2, amount));
            receiver.ChatMessage(Lang(LangKeys.CoinFlipRequestReceived, null, sender.displayName, amount * 2, amount));
        }

        private class RequestHandler
        {
            public ulong Sender { get; set; }
            public ulong Receiver { get; set; }
            public int BetAmount { get; set; }

            public RequestHandler(ulong sender, ulong receiver, int amount)
            {
                Sender = sender;
                Receiver = receiver;
                BetAmount = amount;
            }
        }
        
        private ulong GetSender(BasePlayer receiver)
        {
            foreach (RequestHandler requestHandler in _requests.Values)
            {
                if (requestHandler.Receiver == receiver.userID)
                {
                    return requestHandler.Sender;
                }
            }

            return 0;
        }

        private int GetBetAmount(BasePlayer receiver)
        {
            foreach (RequestHandler requestHandler in _requests.Values)
            {
                if (requestHandler.Receiver == receiver.userID)
                {
                    return requestHandler.BetAmount;
                }
            }

            return 0;
        }
        
        private bool HasPendingRequest(BasePlayer receiver)
        {
            foreach (RequestHandler requestHandler in _requests.Values)
            {
                if (requestHandler.Receiver == receiver.userID)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region CoinFlip Handler

        private readonly List<CoinFlipHandler> _coinFlips = new List<CoinFlipHandler>();

        private class CoinFlipHandler
        {
            public List<BasePlayer> Players { get; set; }
            private Timer Timer { get; set; }

            public CoinFlipHandler(BasePlayer player1, BasePlayer player2, int betAmount)
            {
                _pluginInstance._coinFlips.Add(this);
                Players = new List<BasePlayer>
                {
                    player1,
                    player2
                };

                BasePlayer winner = Players.GetRandom();
                SendMessage(_pluginInstance.Lang(LangKeys.CoinFlipStarting, null, betAmount, player1.displayName, player2.displayName));
                int count = 5;
                Timer = _pluginInstance.timer.Every(1f, () =>
                {
                    count--;
                    if (count == 0)
                    {
                        Timer?.Destroy();
                        SendMessage(_pluginInstance.Lang(LangKeys.CoinFlipWon, null, winner.displayName, betAmount));
                        _pluginInstance.SetRp(winner.userID, _pluginInstance.CheckRp(winner.userID) + betAmount);
                        _pluginInstance._pluginData.CachedCoinFlipBets.Remove(player1.userID);
                        _pluginInstance._pluginData.CachedCoinFlipBets.Remove(player2.userID);
                        _pluginInstance._coinFlips.Remove(this);
                        return;
                    }
                    
                    SendMessage($"{count}...");
                });
            }

            private void SendMessage(string message)
            {
                foreach (BasePlayer player in Players)
                {
                    player.ChatMessage(message);
                }
            }
        }

        private bool IsInCoinFlip(BasePlayer player)
        {
            foreach (CoinFlipHandler coinFlipHandler in _coinFlips)
            {
                if (coinFlipHandler.Players.Contains(player))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Jackpot Handler

        private class JackpotHandler
        {
            private Hash<BasePlayer, int> PlayerBets { get; set; } = new Hash<BasePlayer, int>();
            public int Pot { get; set; }
            private Timer Timer { get; set; }
            public DateTime RemainingTime { get; set; }

            public JackpotHandler()
            {
                RemainingTime = DateTime.UtcNow.AddSeconds(_pluginInstance._pluginConfig.JackpotEndFrequency);
                Timer = _pluginInstance.timer.Every(_pluginInstance._pluginConfig.JackpotEndFrequency, EndRaffle);
            }

            public void AddBet(BasePlayer player, int amount)
            {
                Pot += amount;
                if (PlayerBets.ContainsKey(player))
                {
                    _pluginInstance.Server.Broadcast(_pluginInstance.Lang(LangKeys.PlacedMultipleBets, null, PlayerBets[player], PlayerBets[player] + amount));
                    PlayerBets[player] += amount;
                }
                else
                {
                    PlayerBets[player] += amount;
                    _pluginInstance.Server.Broadcast(_pluginInstance.Lang(LangKeys.PlacedBets, null, PlayerBets.Count, Pot));
                }
                
                _pluginInstance.SetRp(player.userID, _pluginInstance.CheckRp(player.userID) - amount);
                if (_pluginInstance._pluginData.CachedJackpotBets.ContainsKey(player.userID))
                {
                    _pluginInstance._pluginData.CachedJackpotBets[player.userID] += amount;
                }
                else
                {
                    _pluginInstance._pluginData.CachedJackpotBets[player.userID] = amount;
                }
                
                player.ChatMessage(_pluginInstance.Lang(LangKeys.PlacedBet, null, amount, $"{GetPercentage(player):0.00}"));

                _pluginInstance.SaveData();
            }

            private BasePlayer GetWinner()
            {
                int potAmount = UnityEngine.Random.Range(0, Pot);
                int currentAmount = 0;
                foreach (KeyValuePair<BasePlayer, int> playerBet in PlayerBets)
                {
                    currentAmount += playerBet.Value;
                    if (currentAmount > potAmount)
                    {
                        return playerBet.Key;
                    }
                }

                return null;
            }
            
            private double GetPercentage(BasePlayer player)
            {
                return PlayerBets[player] / (double) Pot * 100;
            }

            public void EndRaffle()
            {
                BasePlayer winner = GetWinner();
                if (winner != null)
                {
                    _pluginInstance.SetRp(winner.userID, _pluginInstance.CheckRp(winner.userID) + Pot);
                    winner.ChatMessage(_pluginInstance.Lang(LangKeys.RaffleWon, null, Pot));
                    double winChance = GetPercentage(winner);
                    _pluginInstance.Server.Broadcast(_pluginInstance.Lang(LangKeys.RaffleEnd, null, winner.displayName, Pot, $"{winChance:0.00}"));
                    _pluginInstance._pluginData.Winners.Add(new WinnerData(winner, Pot, winChance));
                }
                else
                {
                    _pluginInstance.Server.Broadcast(_pluginInstance.Lang(LangKeys.RaffleEndNoWinner));
                }
                
                Timer?.Destroy();
                _pluginInstance._pluginData.CachedJackpotBets.Clear();
                _pluginInstance._currentJackpot = null;
                _pluginInstance.timer.Once(_pluginInstance._pluginConfig.JackpotFrequency, () =>
                {
                    _pluginInstance._currentJackpot = new JackpotHandler();
                });
            }
        }

        #endregion
    }
}