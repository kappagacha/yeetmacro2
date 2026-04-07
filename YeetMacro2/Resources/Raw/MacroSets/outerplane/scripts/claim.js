// @raw-script
// @tags=favorites
// @position=1

function claim(type = '') {
	if (!type) type = settings.claim.type.Value;

	switch (type) {
		case 'antiparticle':
			claimAntiparticle();
			break;
		case 'arenaRewards':
			claimArenaRewards();
			break;
		case 'coinExchange':
			claimCoinExchange();
			break;
		case 'dailyMissions':
			claimDailyMissions();
			break;
		case 'eventDailyFireworks':
			claimEventDailyFireworks();
			break;
		case 'eventDailyMissions':
			claimEventDailyMissions();
			break;
		case 'eventDailyMissions2':
			claimEventDailyMissions2();
			break;
		case 'freeRecruit':
			claimFreeRecruit();
			break;
		case 'guildBuff':
			claimGuildBuff();
			break;
		case 'mailboxExpiring':
			claimMailbox('expiring');
			break;
		case 'mailboxNormal':
			claimMailbox('normal');
			break;
		case 'mailboxProduct':
			claimMailbox('product');
			break;
		case 'worldBossRewards':
			claimWorldBossRewards();
			break;
	}
}

function claimAntiparticle() {
	const popupPatterns = [patterns.general.tapEmptySpace, settings.goToLobby.userClickPattern.Value, patterns.general.exitCheckIn, patterns.lobby.popup.doNotShowAgainToday];
	const loopPatterns = [patterns.lobby.level, patterns.titles.base, ...popupPatterns];
	const daily = dailyManager.GetCurrentDaily();

	const isLastRunWithinHour = (Date.now() - settings.claim.antiparticle.lastRun.Value.ToUnixTimeMilliseconds()) / 3_600_000 < 1;

	if (isLastRunWithinHour && !settings.claim.antiparticle.forceRun.Value) {
		return 'Last run was within the hour. Use forceRun setting to override check';
	}

	while (macroService.IsRunning) {
		const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.arena.defendReport.close });
		switch (loopResult.Path) {
			case 'general.tapEmptySpace':
			case 'settings.goToLobby.userClickPattern':
			case 'general.exitCheckIn':
			case 'lobby.popup.doNotShowAgainToday':
				goToLobby();
				break;
			case 'lobby.level':
				logger.info('claimAntiparticle: click base tab');
				const claimRewardsResult = macroService.FindPattern(patterns.tabs.base.claimRewards);
				if (claimRewardsResult.IsSuccess) {
					macroService.PollPattern(patterns.tabs.base.claimRewards, { DoClick: true, PredicatePattern: patterns.tabs.base.claimRewards.receiveReward });

					const specialRewardNotificationResult = macroService.PollPattern(patterns.tabs.base.claimRewards.specialReward.notification, { TimeoutMs: 2_000 });
					if (specialRewardNotificationResult.IsSuccess) {
						macroService.PollPattern(patterns.tabs.base.claimRewards.specialReward, { DoClick: true, PredicatePattern: patterns.tabs.base.claimRewards.specialReward.free });
						macroService.PollPattern(patterns.tabs.base.claimRewards.specialReward.free, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
						macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.tabs.base.claimRewards.receiveReward });
					}

					macroService.PollPattern(patterns.tabs.base.claimRewards.receiveReward, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
					macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.tabs.base.claimRewards.receiveReward.disabled });

					if (macroService.IsRunning) {
						daily.claim.antiparticle.count.Count++;
						settings.claim.antiparticle.lastRun.Value = new Date().toISOString();
					}

					macroService.PollPattern(patterns.tabs.base.claimRewards.receiveReward.disabled, { DoClick: true, ClickOffset: { X: 600 }, PredicatePattern: patterns.lobby.level });
					return;
				}

				macroService.ClickPattern(patterns.tabs.base);
				sleep(500);
				break;
			case 'titles.base':
				logger.info('claimAntiparticle: claim antiparticle');
				const antiParticleResult = macroService.PollPattern(patterns.base.antiparticle, { DoClick: true, PredicatePattern: [patterns.base.antiparticle.receiveReward, patterns.base.antiparticle.receiveReward.disabled] });

				const baseSpecialRewardNotificationResult = macroService.PollPattern(patterns.base.antiparticle.specialReward.notification, { TimeoutMs: 2_000 });
				if (baseSpecialRewardNotificationResult.IsSuccess) {
					macroService.PollPattern(patterns.base.antiparticle.specialReward, { DoClick: true, PredicatePattern: patterns.base.antiparticle.specialReward.free });
					macroService.PollPattern(patterns.base.antiparticle.specialReward.free, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
					macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: [patterns.base.antiparticle.receiveReward, patterns.base.antiparticle.receiveReward.disabled] });
				}

				if (antiParticleResult.PredicatePath === 'base.antiparticle.receiveReward.disabled') {
					return;
				}

				macroService.PollPattern(patterns.base.antiparticle.receiveReward, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
				macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.base.antiparticle.receiveReward.disabled });

				if (macroService.IsRunning) {
					daily.claim.antiparticle.count.Count++;
					settings.claim.antiparticle.lastRun.Value = new Date().toISOString();
				}
				return;
		}
		sleep(1_000);
	}
}

function claimArenaRewards() {
	const loopPatterns = [patterns.lobby.level, patterns.event.close];
	const daily = dailyManager.GetCurrentDaily();

	if (daily.claim.arenaRewards.done.IsChecked) {
		return "Script already completed. Uncheck done to override daily flag.";
	}

	while (macroService.IsRunning) {
		const loopResult = macroService.PollPattern(loopPatterns);
		switch (loopResult.Path) {
			case 'lobby.level':
				logger.info('claimArenaRewards: click event');
				macroService.PollPattern(patterns.lobby.event, { DoClick: true, ClickOffset: { Y: -30 }, PredicatePattern: patterns.event.close });
				sleep(500);
				break;
			case 'event.close':
				logger.info('claimArenaRewards: claim rewards');
				macroService.PollPattern(patterns.event.arenaReward, { SwipePattern: patterns.event.swipeDown });
				macroService.PollPattern(patterns.event.arenaReward, { DoClick: true, PredicatePattern: patterns.event.arenaReward.utc });

				let notificationResult = macroService.PollPattern(patterns.event.arenaReward.notification, { TimeoutMs: 3_000 });
				while (notificationResult.IsSuccess) {
					macroService.PollPattern(patterns.event.arenaReward.notification, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace, ClickOffset: { X: -40, Y: 40 } });
					macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.event.arenaReward.utc });
					notificationResult = macroService.PollPattern(patterns.event.arenaReward.notification, { TimeoutMs: 3_000 });
				}

				const claimedAllResult = macroService.FindPattern(patterns.event.arenaReward.claimedAll);

				if (claimedAllResult.IsSuccess && macroService.IsRunning) {
					daily.claim.arenaRewards.done.IsChecked = true;
				}
				return;
		}
		sleep(1_000);
	}
}

function claimCoinExchange() {
	const loopPatterns = [patterns.lobby.level, patterns.event.close];
	const daily = dailyManager.GetCurrentDaily();
	if (daily.claim.coinExchange.done.IsChecked) {
		return "Script already completed. Uncheck done to override daily flag.";
	}

	while (macroService.IsRunning) {
		const loopResult = macroService.PollPattern(loopPatterns);
		switch (loopResult.Path) {
			case 'lobby.level':
				logger.info('claimCoinExchange: click event');
				macroService.PollPattern(patterns.lobby.event, { DoClick: true, ClickOffset: { Y: -30 }, PredicatePattern: patterns.event.close });
				sleep(500);
				break;
			case 'event.close':
				logger.info('claimCoinExchange: claim rewards');
				macroService.PollPattern(patterns.event.coinExchangeShop, { SwipePattern: patterns.event.swipeDown });
				macroService.PollPattern(patterns.event.coinExchangeShop, { DoClick: true, PredicatePattern: patterns.event.coinExchangeShop.exchange, IntervalDelayMs: 3_000 });
				sleep(2_000);

				let notificationResult = macroService.PollPattern(patterns.event.coinExchangeShop.exchange, { TimeoutMs: 3_000 });
				while (notificationResult.IsSuccess) {
					macroService.PollPattern(patterns.event.coinExchangeShop.exchange, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
					sleep(1_000);
					macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.event.coinExchangeShop.getCoins });

					notificationResult = macroService.PollPattern(patterns.event.coinExchangeShop.exchange, { TimeoutMs: 3_000 });
				}

				if (macroService.IsRunning) {
					daily.claim.coinExchange.done.IsChecked = true;
				}

				return;
		}
		sleep(1_000);
	}
}

function claimDailyMissions() {
	const loopPatterns = [patterns.lobby.level, patterns.titles.mission];
	const daily = dailyManager.GetCurrentDaily();

	if (daily.claim.dailyMissions.done.IsChecked) {
		return "Script already completed. Uncheck done to override daily flag.";
	}

	while (macroService.IsRunning) {
		const loopResult = macroService.PollPattern(loopPatterns);
		switch (loopResult.Path) {
			case 'lobby.level':
				logger.info('claimDailyMissions: click mission');
				macroService.ClickPattern(patterns.tabs.mission);
				sleep(500);
				break;
			case 'titles.mission':
				logger.info('claimDailyMissions: claim final reward');
				const finalNotificationResult = macroService.PollPattern(patterns.mission.finalNotification, { DoClick: true, PredicatePattern: [patterns.general.tapEmptySpace, patterns.mission.claimed] });
				if (finalNotificationResult.PredicatePattern === 'general.tapEmptySpace') {
					macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.mission });
				}
				if (macroService.IsRunning) {
					daily.claim.dailyMissions.done.IsChecked = true;
				}
				return;
		}
		sleep(1_000);
	}
}

function claimEventDailyFireworks() {
	const loopPatterns = [patterns.lobby.level, patterns.event.close];
	const daily = dailyManager.GetCurrentDaily();
	if (daily.claim.eventDailyFireworks.done.IsChecked) {
		return "Script already completed. Uncheck done to override daily flag.";
	}

	while (macroService.IsRunning) {
		const loopResult = macroService.PollPattern(loopPatterns);
		switch (loopResult.Path) {
			case 'lobby.level':
				logger.info('claimEventDailyFireworks: click event');
				macroService.PollPattern(patterns.lobby.event, { DoClick: true, ClickOffset: { Y: -30 }, PredicatePattern: patterns.event.close });
				sleep(500);
				break;
			case 'event.close':
				logger.info('claimEventDailyFireworks: claim rewards');
				macroService.PollPattern(patterns.event.fireworks, { SwipePattern: patterns.event.swipeDown });
				macroService.PollPattern(patterns.event.fireworks, { DoClick: true, PredicatePattern: patterns.event.fireworks.enter, IntervalDelayMs: 3_000 });
				sleep(2_000);

				let notificationResult = macroService.PollPattern(patterns.event.notifications, { TimeoutMs: 3_000 });
				while (notificationResult.IsSuccess) {
					macroService.PollPattern(patterns.event.notifications, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace, ClickOffset: { X: -40, Y: 40 } });
					sleep(1_000);
					macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.event.fireworks.enter });

					notificationResult = macroService.PollPattern(patterns.event.notifications, { TimeoutMs: 3_000 });
				}

				if (macroService.IsRunning) {
					daily.claim.eventDailyFireworks.done.IsChecked = true;
				}

				return;
		}
		sleep(1_000);
	}
}

function claimEventDailyMissions() {
	const loopPatterns = [patterns.lobby.level, patterns.event.close];
	const daily = dailyManager.GetCurrentDaily();

	if (daily.claim.eventDailyMissions.done.IsChecked) {
		return "Script already completed. Uncheck done to override daily flag.";
	}

	while (macroService.IsRunning) {
		const loopResult = macroService.PollPattern(loopPatterns);
		switch (loopResult.Path) {
			case 'lobby.level':
				logger.info('claimEventDailyMissions: click event');
				macroService.PollPattern(patterns.lobby.event, { DoClick: true, ClickOffset: { Y: -30 }, PredicatePattern: patterns.event.close });
				sleep(500);
				break;
			case 'event.close':
				logger.info('claimEventDailyMissions: claim rewards');
				macroService.PollPattern(patterns.event.daily, { SwipePattern: patterns.event.swipeDown });
				macroService.PollPattern(patterns.event.daily, { DoClick: true, PredicatePattern: patterns.event.daily.info, IntervalDelayMs: 3_000 });
				sleep(2_000);

				let notificationResult = macroService.PollPattern(patterns.event.daily.anniversary.notification, { TimeoutMs: 3_000 });
				while (notificationResult.IsSuccess) {
					macroService.PollPattern(patterns.event.daily.anniversary.notification, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace, ClickOffset: { X: -40, Y: 40 } });
					macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.event.daily.info });

					notificationResult = macroService.PollPattern(patterns.event.daily.anniversary.notification, { TimeoutMs: 3_000 });
				}

				const coinExchangeResult = macroService.PollPattern([patterns.event.daily.coinExchange.coin, patterns.event.roulette, patterns.event.rockPaperScissors, patterns.event.drawACapsule], { SwipePattern: patterns.event.swipeDown });

				if (coinExchangeResult.Path === 'event.daily.coinExchange.coin') {
					doCoinExchange();
				} else if (coinExchangeResult.Path === 'event.roulette') {
					doRoulette();
				} else if (coinExchangeResult.Path === 'event.rockPaperScissors') {
					doRockPaperScissors();
				} else if (coinExchangeResult.Path === 'event.drawACapsule') {
					doDrawACapsule();
				}

				if (macroService.IsRunning) {
					daily.claim.eventDailyMissions.done.IsChecked = true;
				}

				return;
		}
		sleep(1_000);
	}

	function doCoinExchange() {
		macroService.PollPattern(patterns.event.daily.coinExchange.coin, { DoClick: true, PredicatePattern: patterns.event.daily.coinExchange.getCoins });
		sleep(2_000);

		let notificationResult = macroService.PollPattern(patterns.event.coinExchangeShop.exchange, { TimeoutMs: 3_000 });
		while (notificationResult.IsSuccess) {
			macroService.PollPattern(patterns.event.coinExchangeShop.exchange, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
			sleep(1_000);
			macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.event.coinExchangeShop.getCoins });

			notificationResult = macroService.PollPattern(patterns.event.coinExchangeShop.exchange, { TimeoutMs: 3_000 });
		}
	}

	function doRoulette() {
		macroService.PollPattern(patterns.event.roulette, { DoClick: true, PredicatePattern: patterns.event.coinsHeld });
		sleep(2_000);
		let numCoins = macroService.FindText(patterns.event.numCoins)?.replace(/[^0-9]/g, '');
		while (macroService.IsRunning && !numCoins) {
			sleep(200);
			numCoins = macroService.FindText(patterns.event.numCoins)?.replace(/[^0-9]/g, '');
		}

		while (macroService.IsRunning && numCoins > 0) {
			macroService.PollPattern(patterns.event.roulette.start, { DoClick: true, ClickOffset: { Y: 300 }, PredicatePattern: patterns.general.tapEmptySpace });
			macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.event.coinsHeld });
			numCoins = macroService.FindText(patterns.event.numCoins)?.replace(/[^0-9]/g, '');
		}
	}

	function doRockPaperScissors() {
		const rockPaperScissors = ['rock', 'paper', 'scissor'];
		macroService.PollPattern(patterns.event.rockPaperScissors, { DoClick: true, PredicatePattern: patterns.event.coinsHeld });
		sleep(2_000);
		let numCoins = macroService.FindText(patterns.event.numCoins)?.replace(/[^0-9]/g, '');
		while (macroService.IsRunning && !numCoins) {
			sleep(200);
			numCoins = macroService.FindText(patterns.event.numCoins)?.replace(/[^0-9]/g, '');
		}

		while (macroService.IsRunning && numCoins > 0) {
			const randomNumber = macroService.Random(0, 2);
			const rockPaperOrScissorPattern = patterns.event.rockPaperScissors[rockPaperScissors[randomNumber]];
			macroService.PollPattern(rockPaperOrScissorPattern, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
			macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.event.coinsHeld });
			numCoins = macroService.FindText(patterns.event.numCoins)?.replace(/[^0-9]/g, '');
		}
	}

	function doDrawACapsule() {
		macroService.PollPattern(patterns.event.drawACapsule, { DoClick: true, PredicatePattern: patterns.event.coinsHeld });
		sleep(2_000);
		let numCoins = macroService.FindText(patterns.event.numCoins)?.replace(/[^0-9]/g, '');
		while (macroService.IsRunning && !numCoins) {
			sleep(200);
			numCoins = macroService.FindText(patterns.event.numCoins)?.replace(/[^0-9]/g, '');
		}

		while (macroService.IsRunning && numCoins > 0) {
			macroService.PollPattern(patterns.event.drawACapsule.draw, { DoClick: true, PredicatePattern: patterns.event.drawACapsule.ok });
			macroService.PollPattern(patterns.event.drawACapsule.ok, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
			macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.event.coinsHeld });
			numCoins = macroService.FindText(patterns.event.numCoins)?.replace(/[^0-9]/g, '');
		}
	}

	//function doSpinTheWheel() {
	//	macroService.PollPattern(patterns.event.spinTheWheel, { DoClick: true, PredicatePattern: patterns.event.coinsOwned });
	//	sleep(2_000);
	//	let numCoins = macroService.FindText(patterns.event.numCoins)?.replace(/[^0-9]/g, '');
	//	while (macroService.IsRunning && !numCoins) {
	//		sleep(200);
	//		numCoins = macroService.FindText(patterns.event.numCoins)?.replace(/[^0-9]/g, '');
	//	}

	//	while (macroService.IsRunning && numCoins > 0) {
	//		macroService.PollPattern(patterns.event.spinTheWheel.start, { DoClick: true, PredicatePattern: patterns.event.confirm });
	//		macroService.PollPattern(patterns.event.confirm, { DoClick: true, PredicatePattern: patterns.event.coinInfo });
	//		numCoins = macroService.FindText(patterns.event.numCoins)?.replace(/[^0-9]/g, '');
	//	}
	//}
}

function claimEventDailyMissions2() {
	//const loopPatterns = [patterns.lobby.level, patterns.event.close];
	const loopPatterns = [patterns.lobby.level, patterns.festival.title];
	const daily = dailyManager.GetCurrentDaily();

	if (daily.claim.eventDailyMissions2.done.IsChecked) {
		return "Script already completed. Uncheck done to override daily flag.";
	}

	//goToLobby();
	//refillStamina(50);

	while (macroService.IsRunning) {
		const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.general.tapEmptySpace });
		switch (loopResult.Path) {
			case 'lobby.level':
				logger.info('claimEventDailyMissions2: click event');
				macroService.ClickPattern(patterns.festival);
				sleep(500);
				break;
			case 'festival.title':
				let demiurgeContractNotificationResult = macroService.PollPattern(patterns.festival.demiurgeContract.notification, { TimeoutMs: 3_000 });
				while (demiurgeContractNotificationResult.IsSuccess) {
					macroService.PollPattern(patterns.festival.demiurgeContract.notification, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace, ClickOffset: { X: -20, Y: 20 } });
					macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.festival.title });
					demiurgeContractNotificationResult = macroService.PollPattern(patterns.festival.demiurgeContract.notification, { TimeoutMs: 3_000 });
				}

				//macroService.PollPattern(patterns.festival.festivalBingo, { DoClick: true, PredicatePattern: patterns.festival.festivalBingo.selected });
				//let festivalBingoNotificationResult = macroService.PollPattern(patterns.festival.festivalBingo.notification, { TimeoutMs: 3_000 });
				//while (festivalBingoNotificationResult.IsSuccess) {
				//	macroService.PollPattern(patterns.festival.festivalBingo.notification, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace, ClickOffset: { X: -40, Y: 40 } });
				//	macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.festival.title });
				//	festivalBingoNotificationResult = macroService.PollPattern(patterns.festival.festivalBingo.notification, { TimeoutMs: 3_000 });
				//}

				//macroService.PollPattern(patterns.festival.showdown, { DoClick: true, PredicatePattern: patterns.festival.showdown.selected });
				//macroService.PollPattern(patterns.festival.showdown.heroDeployment, { DoClick: true, PredicatePattern: patterns.battle.enter });

				//selectTeamAndBattle("Current", { targetNumBattles: 10 });

				if (macroService.IsRunning) {
					daily.claim.eventDailyMissions2.done.IsChecked = true;
				}

				return;
		}
		sleep(1_000);
	}
}

function claimFreeRecruit() {
	const loopPatterns = [patterns.lobby.level, patterns.titles.recruit];
	const daily = dailyManager.GetCurrentDaily();
	if (daily.claim.freeRecruit.done.IsChecked) {
		return "Script already completed. Uncheck done to override daily flag.";
	}

	while (macroService.IsRunning) {
		const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.arena.defendReport.close });
		switch (loopResult.Path) {
			case 'lobby.level':
				logger.info('claimFreeRecruit: click recruit tab');
				const recruitNotificationResult = macroService.PollPattern(patterns.tabs.recruit.notification, { TimeoutMs: 2_000 });
				if (recruitNotificationResult.IsSuccess) {
					macroService.ClickPattern(patterns.tabs.recruit);
				} else {	// already claimed
					return;
				}
				sleep(500);
				break;
			case 'titles.recruit':
				logger.info('claimFreeRecruit: claim recruit');
				for (let i = 0; i < 2; i++) {
					const swipeResult = macroService.PollPattern(patterns.recruit.notification, { SwipePattern: patterns.general.leftPanelSwipeDown, TimeoutMs: 5_000 });
					if (!swipeResult.IsSuccess) {
						throw new Error('Unable to find notification');
					}
					sleep(1_000);
					//macroService.PollPattern(patterns.recruit.normal, { DoClick: true, PredicatePattern: patterns.recruit.normal.ticket });
					macroService.PollPattern(patterns.recruit.notification, { DoClick: true, PredicatePattern: patterns.recruit.normal.free });
					macroService.PollPattern(patterns.recruit.normal.free, { DoClick: true, PredicatePattern: patterns.recruit.prompt.ok });
					macroService.PollPattern(patterns.recruit.prompt.ok, { DoClick: true, ClickPattern: patterns.recruit.skip, PredicatePattern: patterns.recruit.prompt.ok2 });
					macroService.PollPattern(patterns.recruit.prompt.ok2, { DoClick: true, InversePredicatePattern: patterns.recruit.prompt.ok2 });
				}

				if (macroService.IsRunning) {
					daily.claim.freeRecruit.done.IsChecked = true;
				}
				return;
		}
		sleep(1_000);
	}
}

function claimGuildBuff() {
	const popupPatterns = [patterns.general.tapEmptySpace, settings.goToLobby.userClickPattern.Value, patterns.general.exitCheckIn, patterns.lobby.popup.doNotShowAgainToday];
	const loopPatterns = [patterns.lobby.level, patterns.titles.guildHallOfHonor, patterns.titles.guild, ...popupPatterns];

	const daily = dailyManager.GetCurrentDaily();

	if (daily.claim.guildBuff.done.IsChecked) {
		return "Script already completed. Uncheck done to override daily flag.";
	}

	while (macroService.IsRunning) {
		const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: [patterns.general.tapEmptySpace, patterns.guild.checkIn.ok, patterns.guild.raid.startMessage, patterns.guild.raid.endMessage.ok] });
		switch (loopResult.Path) {
			case 'general.tapEmptySpace':
			case 'settings.goToLobby.userClickPattern':
			case 'general.exitCheckIn':
			case 'lobby.popup.doNotShowAgainToday':
				goToLobby();
				break;
			case 'lobby.level':
				const receiveGuildBuffResult = macroService.PollPattern(patterns.lobby.receiveGuildBuff, { DoClick: true, PredicatePattern: patterns.lobby.receiveGuildBuff.message, TimeoutMs: 3_000 });
				if (receiveGuildBuffResult.IsSuccess) {
					macroService.PollPattern(patterns.lobby.receiveGuildBuff.message, { DoClick: true, PredicatePattern: patterns.lobby.level });

					if (macroService.IsRunning) {
						daily.claim.guildBuff.done.IsChecked = true;
					}

					return;
				}

				logger.info('claimGuildBuff: click guild tab');
				macroService.ClickPattern(patterns.tabs.guild);
				break;
			case 'titles.guild':
				logger.info('claimGuildBuff: click hall of honor');
				macroService.ClickPattern(patterns.guild.hallOfHonor);
				sleep(500);
				break;
			case 'titles.guildHallOfHonor':
				logger.info('claimGuildBuff: click receive guild buff');
				macroService.PollPattern(patterns.guild.hallOfHonor.notification);
				macroService.PollPattern(patterns.guild.hallOfHonor.notification, { DoClick: true, PredicatePattern: patterns.guild.hallOfHonor.recieveMessage });
				macroService.PollPattern(patterns.guild.hallOfHonor.recieveMessage, { DoClick: true, PredicatePattern: patterns.titles.guildHallOfHonor });
				if (macroService.IsRunning) {
					daily.claim.guildBuff.done.IsChecked = true;
				}
				return;
		}
		sleep(1_000);
	}
}

function claimMailbox(type) {
	const loopPatterns = [patterns.lobby.level, patterns.titles.mailbox];
	const daily = dailyManager.GetCurrentDaily();

	if (type === 'expiring') {
		return claimMailboxExpiring();
	} else if (type === 'normal') {
		return claimMailboxNormal();
	} else if (type === 'product') {
		return claimMailboxProduct();
	} else {
		return `Invalid type: ${type}. Expected 'expiring', 'normal', or 'product'.`;
	}

	function claimMailboxExpiring() {
		if (daily.claim.mailboxExpiring.done.IsChecked) {
			return "Script already completed.";
		}

		let done = false;
		while (macroService.IsRunning) {
			const loopResult = macroService.PollPattern(loopPatterns);
			switch (loopResult.Path) {
				case 'lobby.level':
					logger.info('claimMailbox: click mailbox for expiring');
					macroService.ClickPattern(patterns.lobby.mailbox);
					sleep(500);
					break;
				case 'titles.mailbox':
					logger.info('claimMailbox: claim expiring items');

					while (!done) {
						macroService.PollPattern(patterns.mailbox.normal, { DoClick: true, PredicatePattern: patterns.mailbox.normal.selected });
						sleep(3_000);
						const receiveResult = macroService.FindPattern(patterns.mailbox.receive, { Limit: 10 });
						if (!receiveResult.IsSuccess) break;

						done = true;
						for (const p of receiveResult.Points) {
							const dPattern = macroService.ClonePattern(patterns.mailbox.expiration.d, { CenterX: p.X, CenterY: p.Y - 75, Width: 110, Height: 40, PathSuffix: `_x${p.X}_y${p.Y - 75}` });
							const dPatternResult = macroService.FindPattern(dPattern);
							if (!dPatternResult.IsSuccess) {
								macroService.PollPoint(p, { PredicatePattern: patterns.general.tapEmptySpace, IntervalDelayMs: 3_000 });
								macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.mailbox.receive });
								sleep(2_000);
								done = false;
								continue;
							}

							const numberPattern = macroService.ClonePattern(patterns.mailbox.expiration.d, { X: dPatternResult.Point.X - 58, Y: dPatternResult.Point.Y - 10, Width: 50, Height: 31, Path: `patterns.mailbox.expiration.number_text_x${dPatternResult.Point.X - 30}_y${dPatternResult.Point.Y}` });
							const numberText = macroService.FindText(numberPattern, "1234567890");

							if (numberText == 1 || numberText == 2 || numberText > 10) {
								macroService.PollPoint(p, { PredicatePattern: patterns.general.tapEmptySpace, IntervalDelayMs: 3_000 });
								macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.mailbox.receive });
								sleep(2_000);
								done = false;
							}
						}
					}

					if (macroService.IsRunning) {
						daily.claim.mailboxExpiring.done.IsChecked = true;
					}
					return;
			}
			sleep(1_000);
		}
	}

	function claimMailboxNormal() {
		let allStamina = true;
		while (macroService.IsRunning) {
			const loopResult = macroService.PollPattern(loopPatterns);
			switch (loopResult.Path) {
				case 'lobby.level':
					logger.info('claimMailbox: click mailbox for normal');
					macroService.ClickPattern(patterns.lobby.mailbox);
					sleep(500);
					break;
				case 'titles.mailbox':
					logger.info('claimMailbox: claim normal items');
					macroService.PollPattern(patterns.mailbox.normal, { DoClick: true, PredicatePattern: patterns.mailbox.normal.selected });
					sleep(1000);
					while (macroService.IsRunning) {
						const receiveResult = macroService.FindPattern(patterns.mailbox.receive, { Limit: 10 });
						allStamina = true;
						for (const p of receiveResult.Points) {
							const staminaPattern = macroService.ClonePattern(patterns.mailbox.stamina, { CenterY: p.Y, Height: 50 });
							const staminaPatternResult = macroService.FindPattern(staminaPattern);
							if (staminaPatternResult.IsSuccess) {
								continue;
							}
							macroService.PollPoint(p, { PredicatePattern: patterns.general.tapEmptySpace, IntervalDelayMs: 3_000 });
							macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.mailbox });
							sleep(500);
							allStamina = false;
							break;
						}
						if (allStamina) {
							macroService.SwipePattern(patterns.mailbox.swipeDown);
							sleep(1_500);
						}
					}
					return;
			}
			sleep(1_000);
		}
	}

	function claimMailboxProduct() {
		let allStamina = true;
		while (macroService.IsRunning) {
			const loopResult = macroService.PollPattern(loopPatterns);
			switch (loopResult.Path) {
				case 'lobby.level':
					logger.info('claimMailbox: click mailbox for product');
					macroService.ClickPattern(patterns.lobby.mailbox);
					sleep(500);
					break;
				case 'titles.mailbox':
					logger.info('claimMailbox: claim product items');
					macroService.PollPattern(patterns.mailbox.product, { DoClick: true, PredicatePattern: patterns.mailbox.product.selected });
					sleep(1000);
					while (macroService.IsRunning) {
						const receiveResult = macroService.FindPattern(patterns.mailbox.receive, { Limit: 10 });
						allStamina = true;
						for (const p of receiveResult.Points) {
							const staminaPattern = macroService.ClonePattern(patterns.mailbox.stamina, { CenterY: p.Y, Height: 50 });
							const staminaPatternResult = macroService.FindPattern(staminaPattern);
							if (staminaPatternResult.IsSuccess) {
								continue;
							}
							macroService.PollPoint(p, { PredicatePattern: patterns.general.tapEmptySpace, IntervalDelayMs: 3_000 });
							macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.mailbox });
							sleep(500);
							allStamina = false;
							break;
						}
						if (allStamina) {
							macroService.SwipePattern(patterns.mailbox.swipeDown);
							sleep(1_500);
						}
					}
					return;
			}
			sleep(1_000);
		}
	}
}

function claimWorldBossRewards() {
	const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.titles.worldBoss];
	const daily = dailyManager.GetCurrentDaily();

	if (daily.claim.worldBossRewards.done.IsChecked) {
		return "Script already completed. Uncheck done to override daily flag.";
	}

	while (macroService.IsRunning) {
		const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.adventure.doNotSeeFor3days });
		switch (loopResult.Path) {
			case 'lobby.level':
				logger.info('claimWorldBossRewards: click adventure tab');
				macroService.ClickPattern(patterns.tabs.adventure);
				sleep(500);
				break;
			case 'titles.adventure':
				logger.info('claimWorldBossRewards: click world boss');
				const worldBossResult = macroService.FindPattern([patterns.adventure.worldBoss.locked, patterns.adventure.worldBoss]);
				if (worldBossResult.Path === 'adventure.worldBoss.locked') return;

				const worldBossNotificationResult = macroService.PollPattern(patterns.adventure.worldBoss.notification, { TimeoutMs: 3_000 });
				if (worldBossNotificationResult.IsSuccess) {
					macroService.ClickPattern(patterns.adventure.worldBoss);
				} else {	// already claimed
					return;
				}

				sleep(500);
				break;
			case 'titles.worldBoss':
				logger.info('claimWorldBossRewards: claim rewards');
				macroService.PollPattern(patterns.adventure.worldBoss.rewardNotification, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
				macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.worldBoss });

				macroService.PollPattern(patterns.adventure.worldBoss.battleStart, { DoClick: true, PredicatePattern: patterns.adventure.worldBoss.battleRecord });
				macroService.PollPattern(patterns.adventure.worldBoss.battleRecord, { DoClick: true, PredicatePattern: patterns.adventure.worldBoss.battleRecord.restoreTeam });
				macroService.PollPattern(patterns.adventure.worldBoss.battleRecord.restoreTeam, { DoClick: true, PredicatePattern: patterns.adventure.worldBoss.battleRecord.restoreTeam.ok });
				macroService.PollPattern(patterns.adventure.worldBoss.battleRecord.restoreTeam.ok, { DoClick: true, PredicatePattern: patterns.adventure.worldBoss.battleRecord });
				macroService.PollPattern(patterns.adventure.worldBoss.enter, { DoClick: true, PredicatePattern: patterns.adventure.worldBoss.saveAndExit });
				macroService.PollPattern(patterns.adventure.worldBoss.saveAndExit, { DoClick: true, PredicatePattern: patterns.adventure.worldBoss.battleStart });

				macroService.PollPattern(patterns.adventure.worldBoss.challengeReward.notification, { DoClick: true, PredicatePattern: patterns.adventure.worldBoss.challengeReward.notification2 });
				let notificationResult = { IsSuccess: true };
				while (notificationResult.IsSuccess) {
					macroService.PollPattern(patterns.adventure.worldBoss.challengeReward.notification2, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
					macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.event.close });
					notificationResult = macroService.PollPattern(patterns.adventure.worldBoss.challengeReward.notification2, { TimeoutMs: 3_000 });
				}

				if (macroService.IsRunning) {
					daily.claim.worldBossRewards.done.IsChecked = true;
				}
				return;
		}
		sleep(1_000);
	}
}

