[![wakatime](https://wakatime.com/badge/user/9f5e6603-f83c-481f-833b-fe7bf79e0c30/project/018b68c1-e566-4e5c-bee2-82f802d59d27.svg)](https://wakatime.com/badge/user/9f5e6603-f83c-481f-833b-fe7bf79e0c30/project/018b68c1-e566-4e5c-bee2-82f802d59d27)

# TODO:
### features:
- [x] (A) resign
- [x] (A) waiting indicator
- [ ] (C) take back flow
- [x] (A) add player id to header on all requests (maybe auth/cookies?)
- [x] (A) click website name to refresh to main page - refactor needed 
- [x] (A) handler game search
- [x] (A) handle game spectators
- [ ] (A) add interaction to lobby game ids (NOW)
- [x] (A) new game button after game ended
- [ ] (B) exception middleware 
- [ ] (A) do not refresh board if game is not completed or auto resign?
- [ ] (A) your move indicator (NOW)

### refactor:
- [x] (B) return from razor (disc, indicator)
- [x] (A) Home and Layout
- [ ] (C) remove files from template
- [x] (A) lobby update - cache and cache invalidation
- [x] (A) simplify lobby hub
- [x] (A) fix storage override on page refresh
- [x] (B) fix signalR events (remove hacks)
- [x] (A) replace trigger 'every 2s' with ws messages  
- [x] (A) move new game flow to rest requests
- [ ] (A) move from dictionaries to memory cache or db
- [x] (A) move mark move flow to signalR  
 
### signalr:
- [x] (A) replace new game to resign button
- [x] (A) handle player id instead of lobby connection in connect request
- [x] (A) update score  

### ui:
- [x] (A) add player name
- [ ] (A) dark mode
- [ ] (B) hover all column
- [ ] (B) move animation
- [x] (B) change alert to modal
- [x] (A) add player score to lobby and order them by most wins
- [x] (A) shorten game ids
- [x] (A) update colors

### bugs:
- [ ] (B) sometimes it will throw that player don't exist
- [ ] (B) you can click make move outside of the game and exception is thrown  
- [ ] (A) why do I need anti-forgery token? 
- [ ] (A) refresh while game is still on going returns empty page (handle game search and replace dropped connections in group)


# TIME SPENT ON THIS PROJECT:
- 25.10 | 30 min
- 26.10 | 420 min
- 27.10 | 380 min
- 28.10 | 440 min
- 29.10 | 380 min
- 30.10 | 300 min
- 31.10 | 180 min
- 04.11 | 60 min
- 06.11 | 130 min
- 08.11 | 240 min
- 09.11 | 100 min
- 10.11 | 120 min
- 11.11 | 80 min
- 27.12 | 60 min
- 01.03 | 300 min
- 02.03 | 120 min
- 03.03 | 70 min
- 04.03 | 160 min
- 05.03 | 330 min
- 08.03 | 30 min
- 09.03 | 180 min
- 10.03 | 120 min
- 18.03 | 360 min  

## SUM
30 + 420 + 380 + 440 + 380 + 300 + 180 + 60 + 130 + 240 + 100 + 120 + 80 + 60 + 300 + 120 + 70 + 160 + 330 + 30 + 180 + 120 + 360
### 4590

