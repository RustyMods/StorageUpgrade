using System;
using System.Collections.Generic;
using HarmonyLib;

namespace StorageUpgrade;

public static class VersionHandshake
{
    private static bool load;
    public static readonly List<ZRpc> ValidatedPeers;
    internal static string ConnectionError = "";
    
    static VersionHandshake()
    {
        ValidatedPeers = [];
        
        StorageUpgradePlugin.harmony.Patch(
            AccessTools.DeclaredMethod(typeof(ZNet), nameof(ZNet.OnNewConnection)),
            new HarmonyMethod(AccessTools.DeclaredMethod(typeof(VersionHandshake),
                nameof(ZNet_OnNewConnection_Prefix))));
        StorageUpgradePlugin.harmony.Patch(AccessTools.DeclaredMethod(typeof(ZNet), nameof(ZNet.RPC_PeerInfo)),
            new HarmonyMethod(AccessTools.DeclaredMethod(typeof(VersionHandshake), nameof(ZNet_RPC_PeerInfo_Prefix))),
            new HarmonyMethod(AccessTools.DeclaredMethod(typeof(VersionHandshake), nameof(ZNet_RPC_PeerInfo_Postfix))));
        StorageUpgradePlugin.harmony.Patch(
            AccessTools.DeclaredMethod(typeof(FejdStartup), nameof(FejdStartup.ShowConnectError)),
            new HarmonyMethod(AccessTools.DeclaredMethod(typeof(VersionHandshake),
                nameof(FejdStartup_ShowConnectError_Postfix))));
        StorageUpgradePlugin.harmony.Patch(AccessTools.DeclaredMethod(typeof(ZNet), nameof(ZNet.Disconnect)),
            new HarmonyMethod(AccessTools.DeclaredMethod(typeof(VersionHandshake), nameof(ZNet_Disconnect_Prefix))));
    }

    public static void Init() => load = true;
    
    private static void ZNet_OnNewConnection_Prefix(ZNetPeer peer, ref ZNet __instance)
    {
        // Register version check call
        StorageUpgradePlugin.LogDebug("Registering version RPC handler");
        peer.m_rpc.Register($"{StorageUpgradePlugin.ModName}_VersionCheck",
            new Action<ZRpc, ZPackage>(RPC_StorageUpgrade_Version));

        // Make calls to check versions
        StorageUpgradePlugin.LogDebug("Invoking version check");
        ZPackage zpackage = new();
        zpackage.Write(StorageUpgradePlugin.ModVersion);
        peer.m_rpc.Invoke($"{StorageUpgradePlugin.ModName}_VersionCheck", zpackage);
    }

    private static bool ZNet_RPC_PeerInfo_Prefix(ZRpc rpc, ZPackage pkg, ref ZNet __instance)
    {
        if (!__instance.IsServer() || ValidatedPeers.Contains(rpc)) return true;
        // Disconnect peer if they didn't send mod version at all
        StorageUpgradePlugin.LogWarning(
            $"Peer ({rpc.m_socket.GetHostName()}) never sent version or couldn't due to previous disconnect, disconnecting");
        rpc.Invoke("Error", 3);
        return false; // Prevent calling underlying method
    }

    private static void ZNet_RPC_PeerInfo_Postfix(ZNet __instance)
    {
        ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(),
            $"{StorageUpgradePlugin.ModName}RequestAdminSync",
            new ZPackage());
    }

    private static void FejdStartup_ShowConnectError_Postfix(FejdStartup __instance)
    {
        if (__instance.m_connectionFailedPanel.activeSelf)
        {
            __instance.m_connectionFailedError.fontSizeMax = 25;
            __instance.m_connectionFailedError.fontSizeMin = 15;
            __instance.m_connectionFailedError.text += $"\n{ConnectionError}";
        }
    }

    private static void ZNet_Disconnect_Prefix(ZNetPeer peer, ref ZNet __instance)
    {
        if (!__instance.IsServer()) return;
        // Remove peer from validated list
        StorageUpgradePlugin.LogInfo(
            $"Peer ({peer.m_rpc.m_socket.GetHostName()}) disconnected, removing from validated list");
        _ = ValidatedPeers.Remove(peer.m_rpc);
    }
    
    
    public static void RPC_StorageUpgrade_Version(ZRpc rpc, ZPackage pkg)
    {
        var version = pkg.ReadString();

        StorageUpgradePlugin.LogInfo(
            $"Version check, local: {StorageUpgradePlugin.ModVersion},  remote: {version}");
        if (version != StorageUpgradePlugin.ModVersion)
        {
            ConnectionError =
                $"{StorageUpgradePlugin.ModName} Installed: {StorageUpgradePlugin.ModVersion}\n Needed: {version}";
            if (!ZNet.instance.IsServer()) return;
            // Different versions - force disconnect client from server
            StorageUpgradePlugin.LogWarning(
                $"Peer ({rpc.m_socket.GetHostName()}) has incompatible version, disconnecting...");
            rpc.Invoke("Error", 3);
        }
        else
        {
            if (!ZNet.instance.IsServer())
            {
                // Enable mod on client if versions match
                StorageUpgradePlugin.LogInfo("Received same version from server!");
            }
            else
            {
                // Add client to validated list
                StorageUpgradePlugin.LogInfo($"Adding peer ({rpc.m_socket.GetHostName()}) to validated list");
                ValidatedPeers.Add(rpc);
            }
        }
    }
}