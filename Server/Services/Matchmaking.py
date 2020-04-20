import random
import socket
import time
from _thread import *
import threading
from datetime import datetime
import json
import requests
import uuid
import boto3
import decimal

match_port = 55443
clients_lock = threading.Lock()
connected = 0
xStep = 1.5
clients = {}

# this is the receiving message loop 
def connection_loop(sock):
    global xStep
    global connected
    global clients
    global clients_lock

    while True:
        print('loop')
        data, addr = sock.recvfrom(1024)
        data = data.decode("utf-8")
        print('received ' + data)
        # receives the user_id and the level of the player
        user_data = json.loads(data)
        last_level = 0
        clients_lock.acquire()
        if not addr in clients:
            user_data['key'] = addr
            clients[addr] = {}
            clients[addr]['key'] = addr
            clients[addr]['user_id'] = user_data['user_id']
            clients[addr]['level'] = user_data['level']
            clients[addr]['score'] = user_data['score']
            clients[addr]['request_time'] = datetime.now()
            print("Got a message from " + clients[addr]['user_id'] )
            last_level = user_data['level']
            connected = connected + 1
        clients_lock.release()

        # verifies if there is another client with the same level and generates game for both
        for c in clients:
            if c != user_data['key']:
                cur_us = clients[c]
                if cur_us['level'] == last_level:
                    generate_game(sock, cur_us, user_data)
                    break
            
        # verifies if there is a couple of clients who just waited for too long
        client_one = {}
        for c in clients:
            cur_us = clients[c]
            print(cur_us['request_time'])
            time_diff = datetime.now() - cur_us['request_time']
            if(time_diff.seconds >= 30):
                if bool(client_one) :
                    generate_game(sock, client_one, cur_us)
                    break
                else:
                    client_one = cur_us

        #sleep just for a short while before going on again
        time.sleep(0.5)

def generate_game(sock, client_one, client_two):
    global clients_lock
    global clients
    clients_lock.acquire()
    game_id = str(uuid.uuid4())
    dynamodb = boto3.resource('dynamodb')
    table = dynamodb.Table('BattleshipPlayer')
    
    print('game_id for player 1 is: ' + game_id)
    table.update_item(
        Key = {
            'user_id' : client_one['user_id']
        },
        UpdateExpression = 'SET game_id = :val',
        ExpressionAttributeValues={
            ':val': game_id,
        }
    )
    print('game_id for player 1 repeat is: ' + game_id)
    client_one_return = {
        'you': {
            'user_id': client_one['user_id'],
            'level': client_one['level'],
            'score': client_one['score'],
            'start' :  'true'
        },
        'opponent': {
            'user_id': client_two['user_id'],
            'level': client_two['level'],
            'score': client_two['score'],
            'start': 'false',
        },
        'game_id': game_id
    }
    socket_text_msg = json.dumps(client_one_return, cls = CustomJsonEncoder)
    socket_msg = bytes(socket_text_msg, 'utf-8')
    sock.sendto(socket_msg, (client_one['key'][0],client_one['key'][1]))

    print('game_id for player 2 is: ' + game_id)
    table.update_item(
        Key = {
            'user_id' : client_two['user_id']
        },
        UpdateExpression = 'SET game_id = :val',
        ExpressionAttributeValues={
            ':val': game_id,
        }
    )
    print('game_id for player 2 repeat is: ' + game_id)
    client_two_return = {
        'you': {
            'user_id': client_two['user_id'],
            'level': client_two['level'],
            'score': client_two['score'],
            'start': 'false',
        },
        'opponent':  {
            'user_id': client_one['user_id'],
            'level': client_one['level'],
            'score': client_one['score'],
            'start': 'true',
        },
        'game_id': game_id
    }
    socket_text_msg = json.dumps(client_two_return, cls = CustomJsonEncoder)
    socket_msg = bytes(socket_text_msg, 'utf-8')
    sock.sendto(socket_msg, (client_two['key'][0],client_two['key'][1]))

    for k in list(clients.keys()):
        if k == client_one['key']:
            del clients[k]
        if k == client_two['key'] and k in clients and clients[k]:
            del clients[k]
    clients_lock.release()


class CustomJsonEncoder(json.JSONEncoder):
    def default(self, obj):
        if isinstance(obj, decimal.Decimal):
            return float(obj)
        return super(CustomJsonEncoder, self).default(obj)

def main():
    s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    s.bind(('', match_port))
    print('Start listening on ' + str(match_port))
    start_new_thread(connection_loop, (s,))
    while True:
        time.sleep(1)

if __name__ == '__main__':
    print('Starting operations')
    main()
   
   