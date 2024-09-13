# xabbo extension
A multi feature extension for G-Earth.

<img src="https://github.com/user-attachments/assets/11225be4-a8db-4422-8fec-68583d39f320" width="500">

# Features

## General

### Anti idle
Prevents your avatar from idling.

### Anti idle-out
Prevents your avatar from leaving the room when idling for too long.

### Anti trade
Prevents users from initiating a trade with you.

### No turn
Prevents you from turning when clicking another user. If `except when reselecting user` is enabled, you will turn to face another user only if clicking the same user twice within a few seconds.

### No walk
Prevents you from walking. If `turn towards tile clicked` is enabled, you will turn to face that tile instead of moving.

### Click to
Allows you to mute, kick, ban, or bounce (*ban then unban, this kicks the user without displaying a kick message to them*) a user by clicking on them..

### No typing indicator
Prevent your avatar from displaying a typing indicator.

### Mute
Prevent chat from bots, pets, pet commands, wired messages, respects or scratches from showing.

### Show who respected / show total respect count
When a user receives respect, shows who gave the respect and the receiver's total respect count.

### Prevent using furni
Prevent using furni when double clicking.

### Double-click to show info
Shows some information about a furni (name, identifier, coordinates, direction) when double clicking it.

### Double-click to hide
Hides a furni when double clicking it. It can be re-shown by going to the Room > Furni tab, selecting all furni with Ctrl+A, right clicking and selecting show.

### Double-click tele to find link
When double clicking a teleporter, if the linking teleporter is also in the room, that teleporter will animate for a second to show which one is linked.

### Flash window
Flashes the Habbo window when receiving whispers, when a user or friend chats, when a user or friend enters, or when receiving a private message.

### Block HC gift notification
Blocks the useless HC gift notification.

## Figure

### Wardrobe
An infinite wardrobe where you can save your avatar's looks.\
`Import wardrobe` will import your in-game wardrobe into the xabbo wardrobe.\
`Add from clipboard` will add all figure strings found in your clipboard to the wardrobe.

## Room

### Info
Shows information about the current room.

### Users
Shows a list of users in the room.\
Users that are currently trading will show a trading icon next to their name.\
Right click to enable pets/bots being shown in the list.

### Visitors
Shows a log of visitors who have entered/left the room.

### Banlist
Shows all users banned from the room and allows you to unban specific users.

### Furni
Shows a list of all furni in the room. Furni can be shown/hidden, picked up or ejected by selecting them and right clicking.

## Info

### Furni
Shows a list of all furni loaded from furni data.

# Commands

## Application

`/x` opens the application window and brings it to the foreground.

## Furni

`/f[urni] h[ide] <name>` hides furni in the room.
* `/f h duck` will hide furni containing "duck" in its name: "Rubber Duck", "Skeleduck", etc.

`/f[urni] s[how] <name>` shows furni in the room.

`/f[urni] p[ickup] <name>` picks up furni in the room owned by you.

`/f[urni] e[ject] <name>` ejects furni in the room that are not owned by you.

## Moderation

`/mute <name> <duration>(h|m)` mutes a user for the specified number of h(ours) or m(inutes).
* `/mute user 24h` will mute user for 24 hours

`/unmute <name>` unmutes the specified user.

`/kick <name>` kicks the specified user.

`/ban <name> [hour|day|perm]` bans the specified user for an hour, a day or permanently. Duration defaults to an hour. If the user is not in the room they will be added to a temporary list (cleared when leaving the room) and banned when they next enter.

## Effects

`/fx <name>` enables the effect specified by its name. This will not consume an effect, and the effect will not be enabled if it has not been activated.
* `/fx mer` will enable the Merdragon effect. (Rain effect from the Totem set)

`/fxa <name>` activates the effect specified by its name. *This will consume the effect if it is not currently activated.*

`/dropfx` removes an effect applied by a furni.

## Friends

`/find <name>` finds which room a friend is currently in. The name doesn't need to match their full username.

## Info

`/p[rofile] <name>` or `/p id:<id>` opens a user's profile specified by their name or ID.
* `/p 1234` will open user "1234"'s profile, `/p id:1234` will open the profile of the user with ID 1234.

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
