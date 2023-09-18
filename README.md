# Expand World Events

Allows configuring the random event system.

Install on all clients and the server (modding [guide](https://youtu.be/L9ljm2eKLrk)).

Install [Expand World Data](https://valheim.thunderstore.io/package/JereKuusela/Expand_World_Data/).

Some features only require server installation.

## Features

- Add new events and edit existing ones.
- Allows multiple events to be active at the same time.
- Change event frequency.

Server side mode can be enabled from Expand World Data config.

## Configuration

See the [wiki](https://valheim.fandom.com/wiki/Events) for more info about events.

The file `expand_events.cfg` is created when starting the game.

The file `expand_world/expand_events.yaml` is created when loading a world.

### expand_events.cfg

All settings are server side:

- Random event interval (default: `46` minutes): How often the game tries to start a random event.
- Random event change (default: `20` percent): The chance to try starting a random event.
- Multiple events (default: `false`): If enabled, multiple events can be active at the same time.
  - When a new event starts, the previous one stays active (unless too close).
  - Clients receive the closest event.
  - Active events are not saved to the save file. Restarting the server removes active events.
- Minimum distance between events (default: `100` meters): When multiple events are enabled, new events cancel previous events within this distance.

### expand_events.yaml

Client side fields:

- name: Identifier.
  - Multiple events can have the same name. This allows creating multiple configurations.
  - Client side fields always come from the first event. Recommended to not include these in the extra configurations.
- spawns: List of spawned objects.
  - See expand_world_spawns for more info.
  - Usually these have lower spawn times and less restrictions compared to normal spawns.
- startMessage: Message shown on the screen during the event.
- endMessage: Message shown on the screen when the event ends.
- forceMusic: Event music.
- forceEnvironment: Event environment/weather.

Server side fields:

- enabled (default: `true`): Quick way to disable this entry.
- duration (default: `60` seconds): How long the event lasts.
- nearBaseOnly (default: `true`): Only triggers when near 3 player base items.
- pauseIfNoPlayerInArea (default: `true`): The event timer pauses if no player in the area.
- biome: List of required biomes.
- random (default: `true`): The event can happen randomly (unlike boss events which happen when near a boss).
- requiredEnvironments: List of valid environments/weathers. Checked by the server so using `env` command in the client doesn't affect this.
- requiredGlobalKeys: Event becomes available if the world has any of these keys.
- notRequiredGlobalKeys: Event is not available if the world has any of these keys.
- requiredPlayerKeys: Event becomes available if the player has any of these keys.
- notRequiredPlayerKeys: Event is not available if the player has any of these keys.
- requiredKnownItems: Event becomes available if the player knows any of these items.
- notRequiredKnownItems: Event is not available if the player knows any of these items.

## Credits

Thanks for Azumatt for creating the mod icon!

Thanks for blaxxun for creating the server sync!

Sources: [GitHub](https://github.com/JereKuusela/valheim-expand_world_events)
Donations: [Buy me a computer](https://www.buymeacoffee.com/jerekuusela)
