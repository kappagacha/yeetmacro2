// @position=1000
// Go to lobby
logger.info('goToLobby: start');

const userClickPattern = macroService.ClonePattern(settings.goToLobby.userClickPattern.Value, {
	Path: 'settings.goToLobby.userClickPattern',
	OffsetCalcType: 'None'
});

//claimFreeShop
//claimFreeRecruit
//claimGuildBuff
//doArena
// => patterns.general.back
//doBountyHunter
//doBanditChase
//doUpgradeStoneRetrieval
//doSideStory1
//doSideStory2
//doGuildSecurityArea
// => (auto) patterns.battle.setup.enter.ok => patterns.battle.exit => patterns.general.back
// => (sweep) patterns.general.back
//watchAds
// => patterns.stamina.cancel
//doSpecialRequests
// => patterns.challenge.specialRequest.sweepAll.cancel
//claimEventDailyMissions
// => patterns.event.close

const loopPatterns = [patterns.lobby.level, patterns.lobby.expedition];
const clickPatterns = [
	patterns.general.back,
	patterns.battle.setup.enter.ok,
	patterns.battle.exit,
	patterns.stamina.cancel,
	patterns.event.close,
	patterns.challenge.specialRequest.sweepAll.cancel,
	//patterns.lobby.expedition.searchAgain,
	patterns.general.startMessageClose,
	patterns.general.tapEmptySpace,
	patterns.general.exitCheckIn,
	patterns.friends.ok,
	patterns.login.downloadPatch,
	patterns.login.touchToStart,
	userClickPattern
]

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: clickPatterns });
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('goToLobby: done');
			return;
		case 'lobby.expedition':
			logger.info('goToLobby: expedition');
			macroService.PollPattern(patterns.lobby.expedition.researchAll, { DoClick: true, ClickPattern: patterns.general.tapEmptySpace, PredicatePattern: patterns.lobby.expedition.researchAll.disabled });
			macroService.PollPattern(patterns.lobby.expedition.close, { DoClick: true, ClickPattern: patterns.general.tapEmptySpace, PredicatePattern: patterns.lobby.level });
			logger.info('goToLobby: done');
			return;
	}
	sleep(1_000);
}
