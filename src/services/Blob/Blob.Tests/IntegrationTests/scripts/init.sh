#!/bin/sh

awslocal dynamodb create-table \
    --table-name blob \
    --key-schema AttributeName=Id,KeyType=HASH \
    --attribute-definitions AttributeName=Id,AttributeType=S \
    --provisioned-throughput ReadCapacityUnits=10,WriteCapacityUnits=5

awslocal s3 mb s3://blob

exit 0
