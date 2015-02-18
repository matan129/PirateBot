#!/bin/sh

python "./lib/playgame.py" -e -E -d --loadtime 10000 --engine_seed 42 --player_seed 42 --log_dir lib/game_logs --map_file "./maps/default_map.map" "$1"  "$2"