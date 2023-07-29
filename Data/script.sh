while true; do

    curl -k -L -s --compressed -o coin.txt https://coinmarketcap.com

    ./a.out

    # Sleep for 20 seconds
    sleep 60
done