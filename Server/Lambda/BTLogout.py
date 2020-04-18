import json
import datetime 
import boto3 
import uuid

dynamodb = boto3.resource('dynamodb')

def lambda_handler(event, context):
    table = dynamodb.Table('BattleshipPlayer')
    # we'll start by saving entries
    if event['httpMethod'] == 'GET':
        if event['queryStringParameters']:
            params = event['queryStringParameters']
            if 'Username' in params :
                user_id = params['Username']
                resp_user = table.get_item(Key={'user_id':user_id})
                if 'Item' in resp_user:
                    table.update_item(
                        Key = {
                            'user_id' : user_id
                        },
                        UpdateExpression = 'SET session_tk = :val, login_date = :currentDate',
                        ExpressionAttributeValues={
                            ':val': '-',
                            ':currentDate': '-',
                        }
                    )
                    return {
                        'statusCode': 200,
                        'body': '{"result": "User logged out successfully"}'
                    }
                return error_object('Error - user not found')
            else:
                return error_object('Bad Request - needs to send Username in the request querystring')
        else :
            # BAD Request
            return error_object('Bad Request - needs to send a request querystring with the Username')
    else:
        return error_object('Only GET is supported')

def error_object(error_message):
    return {
        'statusCode': 200,
        'body': '{"error":"' + error_message + '"}' 
    }
