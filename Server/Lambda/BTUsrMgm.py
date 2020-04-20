import json
import datetime 
import boto3
import decimal
import uuid
from boto3.dynamodb.conditions import Key, Attr

dynamodb = boto3.resource('dynamodb')

def lambda_handler(event, context):
    table = dynamodb.Table('BattleshipPlayer')
    # we'll start by saving new entries
    if event['httpMethod'] == 'PUT':
        if 'body' in event:
            request_text = event['body']
            body = json.loads(request_text)
            if 'Username' in body and 'Password' in body and body['Username'] and body['Password']:
                user_id = body['Username']
                resp_user = table.get_item(Key={'user_id':user_id})
                if 'Item' in resp_user and resp_user['Item']:
                    return error_object('Error - user with username ' + body['Username'] + ' already exists - be creative - get another')
                session_tk = str(uuid.uuid4())
                new_entry = {
                    'user_id' : body['Username'],
                    'password': body['Password'],
                    'score': 1200,
                    'wins': 0,
                    'losses':0,
                    'total_matches': 0, 
                    'session_tk': session_tk,
                    'login_date':  datetime.datetime.now().strftime("%Y%m%d%H%M%S"),
                }
                table.put_item(
                    Item = new_entry
                )
                return {
                    'statusCode': 200,
                    'body': json.dumps(new_entry, cls = CustomJsonEncoder)
                }
            else:
                return error_object('Bad Request - needs to send Username and Password in the request body and they must not be empty')
        else :
            # BAD Request
            return error_object('Bad Request - needs to send a request body')        
    elif event['httpMethod'] == 'POST':
        if 'body' in event:
            request_text = event['body']
            body = json.loads(request_text)
            if 'Username' in body and 'SessionToken' in body and 'Score' in body and body['Username'] and body['SessionToken'] and body['Score']:
                user_id = body['Username']
                resp_user = table.get_item(Key={'user_id':user_id})
                if 'Item' in resp_user and resp_user['Item']:
                    user_data = resp_user['Item']
                    print(body['SessionToken'])
                    print(user_data['session_tk'])
                    if user_data['session_tk'] != body['SessionToken']:
                        return error_object('Wrong session ID are you trying to hack me?')
                    final_score = decimal.Decimal(user_data['score']) + decimal.Decimal(body['Score'])
                    final_wins = user_data['wins']
                    final_losses = user_data['losses']
                    final_total_matches = user_data['total_matches'] + 1
                    if body['Score'] > 0:
                        final_wins += 1
                    elif body['Score'] < 0:
                        final_losses += 1
                    table.update_item(
                        Key = {
                            'user_id' : user_id
                        },
                        UpdateExpression = 'SET score = :final_score, wins = :final_wins, losses = :final_losses, total_matches = :final_total_matches ',
                        ExpressionAttributeValues={
                            ':final_score': final_score,
                            ':final_wins': final_wins,
                            ':final_losses': final_losses,
                            ':final_total_matches': final_total_matches,
                        }
                    )
                    return {
                        'statusCode': 200,
                        'body': '{"result": "User score updated successfully"}'
                    }
                else:
                    return error_object('Error - user not found')
            else:
                return error_object('Bad Request - needs to send Username, Score, and SessionToken in the request body')
        else :
            # BAD Request
            return error_object('Bad Request - needs to send a request body')
    elif event['httpMethod'] == 'GET':
        #Now we are querying the database
        if event['queryStringParameters']:
            params = event['queryStringParameters']
            if 'Username' in params and params['Username']:
                user_id = params['Username']
                resp_user = table.get_item(Key={'user_id':user_id})
                if 'Item' in resp_user:
                    return {
                        'statusCode': 200,
                        'body': json.dumps(resp_user['Item'], cls = CustomJsonEncoder)
                    }
                return error_object('Error - user not found')
            else:
                return error_object('Bad Request - needs to send Username in the request querystring')
        else :
            # BAD Request
            return error_object('Bad Request - needs to send a request querystring with the Username')
    else:
        return error_object('Only GET, POST, and PUT are supported')

def error_object(error_message):
    return {
        'statusCode': 200,
        'body': '{"error":"' + error_message + '"}' 
    }

class CustomJsonEncoder(json.JSONEncoder):
    def default(self, obj):
        if isinstance(obj, decimal.Decimal):
            return float(obj)
        return super(CustomJsonEncoder, self).default(obj)

