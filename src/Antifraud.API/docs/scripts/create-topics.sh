#!/bin/bash
# Wait for kafka to be available
sleep 10
echo "Registering topics"

kafka-topics --bootstrap-server kafka:29092 --create --if-not-exists --topic transactions --partitions 3 --replication-factor 1
kafka-topics --bootstrap-server kafka:29092 --create --if-not-exists --topic transactions-confirmed --partitions 1 --replication-factor 1

echo "Topics registred"