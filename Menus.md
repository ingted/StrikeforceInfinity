# Menu Layout

- MenuStartNewGame
	- SpMenuSituationSelect
		- SpMenuSelectLoadout
			- StartGame!
	- MpMenuCreateOrJoinLobby
		- MpMenuClientJoinLobby
			- MpMenuClientSelectLoadout
				- MpMenuClientLobbyWait
					- [Wait on Server to Start]!
		- MpMenuHostCreateLobby
			- MpMenuHostSituationSelect
				- MpMenuHostSelectLoadout
					- MpMenuHostLobbyWait
						- StartGame!
