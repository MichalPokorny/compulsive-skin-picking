#!/bin/sh
find . -name \*.cs | xargs du -b | cut -f 1 | awk '{ a += $1 } END { print a / 1024 }'
