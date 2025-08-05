// @position=7
const loopPatterns = [patterns.lobby, patterns.menu.guild.title];
const daily = dailyManager.GetCurrentDaily();
if (daily.doGuild.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby':
			logger.info('doGuild: click guild');
			macroService.PollPattern(patterns.menu, { DoClick: true, PredicatePattern: patterns.menu.close });
			if (!macroService.FindPattern(patterns.menu.guild.notification).IsSuccess) {
				if (macroService.IsRunning) daily.doGuild.done.IsChecked = true;
				return;
			}
			macroService.PollPattern(patterns.menu.guild, { DoClick: true, PredicatePattern: patterns.menu.guild.title });
			break;
		case 'menu.guild.title':
			logger.info('doGuild: donate');
			if (settings.doGuild.donate.Value && !daily.doGuild.donate.IsChecked) {
				macroService.PollPattern(patterns.menu.guild.donate, { DoClick: true, PredicatePattern: patterns.menu.guild.donate.close });
				macroService.PollPattern(patterns.menu.guild.donate.gold, { DoClick: true, PredicatePattern: patterns.general.itemsAcquired });
				macroService.PollPattern(patterns.general.itemsAcquired, { DoClick: true, PredicatePattern: patterns.menu.guild.donate.close });

				for (let i = 0; i < 3; i++) {
					macroService.PollPattern(patterns.menu.guild.donate.fairyQueenToken, { DoClick: true, PredicatePattern: patterns.general.itemsAcquired });
					macroService.PollPattern(patterns.general.itemsAcquired, { DoClick: true, PredicatePattern: patterns.menu.guild.donate.close });
				}

				macroService.PollPattern(patterns.menu.guild.donate.close, { DoClick: true, PredicatePattern: patterns.menu.guild.title });

				if (macroService.IsRunning) daily.doGuild.donate.IsChecked = true;
			}

			if (settings.doGuild.checkIn.Value && !daily.doGuild.checkIn.IsChecked) {
				// TODO: need checkIn pattern
				macroService.PollPattern(patterns.menu.guild.checkIn, { DoClick: true, PredicatePattern: patterns.general.itemsAcquired });
				macroService.PollPattern(patterns.general.itemsAcquired, { DoClick: true, PredicatePattern: patterns.menu.guild.title });

				if (macroService.IsRunning) daily.doGuild.checkIn.IsChecked = true;
			}

			let claimRewardResult = macroService.FindPattern(patterns.menu.guild.claimReward);

			while (claimRewardResult.IsSuccess) {
				macroService.PollPattern(patterns.menu.guild.claimReward, { DoClick: true, PredicatePattern: patterns.general.itemsAcquired });
				macroService.PollPattern(patterns.general.itemsAcquired, { DoClick: true, PredicatePattern: patterns.menu.guild.title });

				claimRewardResult = macroService.FindPattern(patterns.menu.guild.claimReward);
			}

			if (settings.doGuild.investGold.Value && !daily.doGuild.investGold.IsChecked) {
				macroService.PollPattern(patterns.menu.guild.investGold, { DoClick: true, PredicatePattern: patterns.general.itemsAcquired });
				macroService.PollPattern(patterns.general.itemsAcquired, { DoClick: true, PredicatePattern: patterns.menu.guild.title });

				if (macroService.IsRunning) daily.doGuild.investGold.IsChecked = true;
			}

			if (settings.doGuild.investGuildCoin.Value && !daily.doGuild.investGuildCoin.IsChecked) {
				macroService.PollPattern(patterns.menu.guild.investGuildCoin, { DoClick: true, PredicatePattern: patterns.general.itemsAcquired });
				macroService.PollPattern(patterns.general.itemsAcquired, { DoClick: true, PredicatePattern: patterns.menu.guild.title });

				if (macroService.IsRunning) daily.doGuild.investGuildCoin.IsChecked = true;
			}

			if (macroService.IsRunning) daily.doGuild.done.IsChecked = true;

			return;
	}
	sleep(1_000);
}