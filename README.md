# Expand World Events

Allows configuring the random event system.

Install on the server (modding [guide](https://youtu.be/L9ljm2eKLrk)).

Install on the clients to access all features.

Install Expand World Data.

## Features

- Add new events and edit existing ones.
- Allows multiple events to be active at the same time.
- Change event frequency.

Server side mode can be enabled from Expand World Data config.

## Server side features

This mod can be used server only, without requiring clients to have it. However only following files can be configured:

- All settings in the `expand_events.cfg`
- Following fields in the `expand_events.yaml`:
  - enabled
  - duration
  - nearBaseOnly
  - biome
  - requiredGlobalKeys
  - notRequiredGlobalKeys
  - pauseIfNoPlayerInArea

### Server side

This mod can be used server only, without requiring clients to have it. However only following files can be configured:

- `expand_dungeons.yaml`: All fields.
- `expand_events.yaml`: Fields enabled, duration, nearBaseOnly, biome, requiredGlobalKeys, notRequiredGlobalKeys, pauseIfNoPlayerInArea, random.
- `expand_locations.yaml`: All fields, but some disabled locations won't work.
- `expand_rooms.yaml`: All fields.
- `expand_vegetation.yaml`: All fields.
- `expand_world.cfg`: Only settings Random event chance, Random event interval and Zone spawners.

When doing this, enable `Server only` on the config to remove version check.

## Configuration

See the [wiki](https://valheim.fandom.com/wiki/Events) for more info about events.

The file `expand_events.cfg` is created when starting the game.

The file `expand_world/expand_events.yaml` is created when loading a world.

### expand_events.cfg

- Random event interval (default: `46` minutes): How often the game tries to start a random event.
- Random event change (default: `20` percent): The chance to try starting a random event.
- Multiple events (default: `false`): If enabled, multiple events can be active at the same time.
  - When a new event starts, the previous one stays active (unless too close).
  - Clients receive the closest event.
  - Active events are not saved to the save file. Restarting the server removes active events.
- Minimum distance between events (default: `100` meters): When multiple events are enabled, new events cancel previous events within this distance.

### expand_events.yaml

- name: Identifier.
  - Multiple events can have the same name. This allows creating multiple configurations.
  - Spawns and other client side fields always come from the first event.
- enabled (default: `false`): Quick way to disable this entry.
- duration (default: `60` seconds): How long the event lasts.
- nearBaseOnly (default: `true`): Only triggers when near 3 player base items.
- biome: List of possible biomes.
- requiredGlobalKeys: Event becomes available if the world has any of these keys.
- notRequiredGlobalKeys: Event is not available if the world has any of these keys.
- requiredPlayerKeys: Event becomes available if the player has any of these keys.
- notRequiredPlayerKeys: Event is not available if the player has any of these keys.
- requiredKnownItems: Event becomes available if the player knows any of these items.
- notRequiredKnownItems: Event is not available if the player knows any of these items.
- startMessage: Message shown on the screen during the event.
- endMessage: The end message.
- forceMusic: Event music.
- forceEnvironment: Event environment/weather.
- requiredEnvironments: List of valid environments/weathers. Checked by the server so using `env` command in the client doesn't affect this.
- spawns: List of spawned objects. See spawns section for more info. Usually these have lower spawn times and less restrictions compared to normal spawns.
- pauseIfNoPlayerInArea (default: `true`): The event timer pauses if no player in the area.
- random (default: `true`): The event can happen randomly (unlike boss events which happen when near a boss).
