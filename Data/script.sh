#!/bin/bash

while true; do
    grep -oP '(?<=<span>).*?(?=</span>)' coin.txt > out.txt

    curl -k -L -s --compressed -o coin.txt https://coinmarketcap.com

    ./main.c

    # Sleep for 20 seconds
    sleep 20
done