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

match_port = 44332
clients_lock = threading.Lock()
game_lock = threading.Lock()
connected = 0
xStep = 1.5
clients = {}
game = {}

# this is the receiving message loop 
def connection_loop(sock):
    global xStep
    global connected
    global clients
    global clients_lock
    global game_lock

    while True:
        print('loop')
        data, addr = sock.recvfrom(1024)
        data = data.decode("utf-8")
        print('received ' + data)
        # receives the user_id and the level of the player
        user_data = json.loads(data)
        game_id = user_data['game_id']
        user_id = user_data['user_id']
        last_level = 0
        if not addr in clients:
            print("Got the first message from " + user_data['user_id'])
            clients_lock.acquire()
            clients[addr] = {}
            clients[addr]['key'] = addr
            clients[addr]['user_id'] = user_id
            clients[addr]['game_id'] = game_id
            clients_lock.release()
            if game_id in game and 'player_one' in game[game_id]:
                game[game_id]['player_two'] = {}
                game[game_id]['player_two']['key'] = addr
                game[game_id]['player_two']['user_id'] = user_id
                
                game[game_id]['player_two']['occupied_cells'] = ''
                game[game_id]['player_two']['bombed_cells'] = ''
                game[game_id]['player_two']['intact_cells'] = 0
                
                clients[addr]['player_key'] = 'player_two'
                print('Setting player 2 for game ' + user_id)
            else:
                game_lock.acquire()
                game[game_id] = {}
                game[game_id]['player_one'] = {}
                game[game_id]['player_one']['key'] = addr
                game[game_id]['player_one']['user_id'] = user_id
                game_lock.release()
                
                # These will be filled when information on completed board arrives
                game[game_id]['player_one']['occupied_cells'] = ''
                game[game_id]['player_one']['bombed_cells'] = ''
                game[game_id]['player_one']['intact_cells'] = 0
                
                clients[addr]['player_key'] = 'player_one'
                print('Setting player 1 for game ' + user_id)
                
            connected = connected + 1
        else:
            # make distinction between messages
            print('received ' + data + ' from ' + clients[addr]['user_id'])
            pl_key = clients[addr]['player_key']
            print('Command Issued: ' + user_data['cmd'])
            oth_pl_key = "player_one"
            if pl_key.find('one') > 0 :
                oth_pl_key = 'player_two'
            print("current player: " + pl_key + " " + game[game_id][pl_key]['user_id'])
            print("other player: " + oth_pl_key + " " + game[game_id][oth_pl_key]['user_id'])
            if user_data['cmd'] == 'table':
                print('Storing table data for ' + pl_key)
                game[game_id][pl_key]['occupied_cells'] = user_data['occupied_cells']
                game[game_id][pl_key]['intact_cells'] = int(user_data['intact_cells'])
                ready_alert = {
                    'cmd':'table',
                    'user_id':user_id,
                    'game_id':game_id,
                }
                table_alert = json.dumps(ready_alert)
                print('Sending this data to' + game[game_id][oth_pl_key]['user_id']  + ': ' + table_alert)
                #Sends data to the other client
                remote_address = game[game_id][oth_pl_key]['key']
                sock.sendto(bytes(table_alert,'utf8'), (remote_address[0], remote_address[1]))
                print('data_send')
                
            elif user_data['cmd'] == 'attack':
                coords = user_data['coordinates']
                attack_field = game[game_id][oth_pl_key]['occupied_cells']
                blown_field = game[game_id][oth_pl_key]['bombed_cells']
                if blown_field.find(coords) < 0 :
                    #this is a new location
                    #sending return attack to both players
                    attack_alert = {
                        'cmd':'attack',
                        'user_id':clients[addr]['user_id'],
                        'coordinates':user_data['coordinates'],
                    }
                    game[game_id][oth_pl_key]['bombed_cells'] += coords
                    hit = "false"
                    final = False
                    if attack_field.find(coords) >=0:
                        hit = "true"
                        game[game_id][oth_pl_key]['intact_cells'] -= 1
                        if game[game_id][oth_pl_key]['intact_cells'] == 0:
                            #sends you lose message
                            print('you lose, ' + game[game_id][oth_pl_key]['user_id'])
                            final = True
                    #sending the attack alert to both clients
                    attack_alert['hit'] = hit
                    if final:
                        attack_alert['win'] = 'true'
                    else:
                        attack_alert['win'] = 'false'
                        
                    attack_alert_string = json.dumps(attack_alert)
                    print('Sending message: ' + attack_alert_string)
                    
                    remote_address = game[game_id][pl_key]['key']
                    sock.sendto(bytes(attack_alert_string,'utf8'), (remote_address[0], remote_address[1]))
                    print('Message sent to : ' + game[game_id][pl_key]['user_id'])
                    
                    remote_address = game[game_id][oth_pl_key]['key']
                    sock.sendto(bytes(attack_alert_string,'utf8'), (remote_address[0], remote_address[1]))
                    print('Message sent to : ' + game[game_id][oth_pl_key]['user_id'])
                    
                    if final:
                        game_lock.acquire()
                        del game[game_id]
                        game_lock.release()

                    
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
   