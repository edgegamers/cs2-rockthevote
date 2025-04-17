using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;

namespace cs2_rockthevote;

// public partial class Plugin {
//     [ConsoleCommand("changemap", "Changes map to specified map")]
//     //[RequiresPermissions("@css/changemap")]
//     public void OnForceChangeMap(CCSPlayerController? player, CommandInfo? command) {
//         if (player == null) return;
//         
//         if (command == null || string.IsNullOrEmpty(command.ArgString)) {
//             player.PrintToChat("You did not specify what map to change to.");
//             return;
//         }
//         
//         bool foundMatch = false;
//         foreach (var map in _changeMapManager._maps)
//         {
//             if (map.Name == command.ArgString) {
//                 foundMatch = true;
//                 _changeMapManager.ScheduleMapChange(command.ArgString, true);
//                 _changeMapManager.ChangeNextMap(true);
//                 break;
//             }
//         }
//
//         if (!foundMatch) {
//             player.PrintToChat("Could not find map that matches the specified name. Did you type it in correctly?");
//         }
//     }
// }