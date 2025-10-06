// @position=1000
// Go to lobby
logger.info('goToLobby: start');

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

const loopPatterns = [patterns.lobby.level, patterns.lobby.expedition, patterns.startScreen.settings, patterns.lobby.popup.close];
const clickPatterns = [
	patterns.general.back,
	patterns.battle.setup.enter.ok,
	patterns.battle.exit,
	patterns.stamina.cancel,
	patterns.event.close,
	patterns.challenge.specialRequest.sweepAll.cancel,
	//patterns.lobby.expedition.searchAgain,
	patterns.general.tapEmptySpace,
	patterns.general.exitCheckIn,
	patterns.friends.ok,
	//patterns.login.downloadPatch,
	settings.goToLobby.userClickPattern.Value,
	patterns.guild.checkIn.ok,
	patterns.ad.cancel
]

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: clickPatterns });
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('goToLobby: done');
			return;
		case 'lobby.popup.close':
			logger.info('goToLobby: close popup');
			macroService.PollPattern(patterns.lobby.popup.doNotShowAgainToday.unchecked, { DoClick: true, PredicatePattern: patterns.lobby.popup.doNotShowAgainToday.checked });
			macroService.PollPattern(patterns.lobby.popup.close, { DoClick: true, InversePredicatePattern: patterns.lobby.popup.close });
			return;
		case 'startScreen.settings':
			macroService.PollPattern(patterns.startScreen.settings, { DoClick: true, ClickOffset: { X: -200 }, InversePredicatePattern: patterns.startScreen.settings });
			break;
		case 'lobby.expedition':
			logger.info('goToLobby: expedition');
			macroService.PollPattern([patterns.lobby.expedition.researchAll, patterns.lobby.expedition.researchAll.disabled], { ClickPattern: patterns.general.tapEmptySpace });
			sleep(3_000);
			const researchAllResult = macroService.FindPattern([patterns.lobby.expedition.researchAll, patterns.lobby.expedition.researchAll.disabled]);
			if (researchAllResult.Path === 'lobby.expedition.researchAll.disabled') {
				macroService.PollPattern(patterns.lobby.expedition.close, { DoClick: true, ClickPattern: patterns.general.tapEmptySpace, PredicatePattern: patterns.lobby.level });
				refillStamina(12);
				startExpeditions();
				goToLobby();
				return;
			}
			macroService.PollPattern(patterns.lobby.expedition.researchAll, { DoClick: true, ClickPattern: patterns.general.tapEmptySpace, PredicatePattern: patterns.lobby.expedition.researchAll.disabled });
			macroService.PollPattern(patterns.lobby.expedition.close, { DoClick: true, ClickPattern: patterns.general.tapEmptySpace, PredicatePattern: patterns.lobby.level });
			logger.info('goToLobby: done');
			return;
	}
	sleep(1_000);
}
