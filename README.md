# b7.Xabbo
[enhanced habbo] A multi feature extension for G-Earth.

# Commands

## Application

`/x` opens the application window and brings it to the foreground.

## Effects

`/fx <name>` enables the effect specified by its name. This will not consume an effect, and the effect will not be enabled if it has not been activated.
- `/fx mer` will enable the Merdragon effect. (Rain effect from the Totem set)

`/fxa <name>` activates the effect specified by its name. *This will consume the effect if it is not currently activated.*

`/dropfx` removes an effect applied by a furni, this works by quickly enabling and disabling the lightsaber effect.

## Friends

`/find <name>` finds which room a friend is currently in. The name doesn't need to match their full username.

## Furni

`/f[urni] h[ide] <name>` hides furni in the room.

- `/f h duck` will hide furni containing "duck" in its name: "Rubber Duck", "Skeleduck", etc.

`/f[urni] s[how] <name>` shows furni in the room.

`/f[urni] p[ickup] <name>` picks up furni in the room owned by you.

`/f[urni] e[ject] <name>` ejects furni in the room that are not owned by you.

## Info

`/p[rofile] <name>` or `/p id:<id>` opens a user's profile specified by their name or ID.

- `/p 1234` will open user "1234"'s profile, `/p id:1234` will open the profile of the user with ID 1234.

`/g[roup] <id>` opens information of the group with the specified ID.

## Turning

`/t[urn] <dir>` makes your avatar turn to a specified direction.\
`dir` can be one of `n`, `ne`, `e`, `se`, `s`, `sw`, `w` or `nw`.

## Moodlights / toners

`/mood` toggles the moodlight on/off.

`/mood settings` opens the moodlight settings.

`/bg` toggles the background toner on/off.

`/bg <hex>` sets the background toner to the specified hex color.

## Rooms

`/go <text>` searches the navigator for the specified text and enters the first room in the results.

`/goto <id>` enters the room with the specified ID.

`/exit` leaves the room.

`/reload` re-enters the current room.

`/trigger` triggers the room entry wired.

## Profile

`/motto <text>` sets your motto to the specified text.

## Cancelling long operations

Use `/cancel` or `/c` to cancel long operations initiated by commands such as picking up furni in a room.
