using VRChatUrlLauncher;

if (args.Length < 1)
{
    Console.WriteLine("Usage: VRChatUrlLauncher <VRChat URL>");
    return;
}

await VRChatIpcClient.SendAsync(args[0]);