﻿// @position=2
// Claim mail
const loopPatterns = [patterns.lobby.level, patterns.titles.mailbox];
const daily = dailyManager.GetCurrentDaily();
if (daily.claimMail.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}
while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('claimMail: click mail');
			macroService.ClickPattern(patterns.lobby.mail);
			break;
		case 'titles.mailbox':
			logger.info('claimMail: claim normal');
			const newNormalResult = macroService.FindPattern(patterns.mailbox.new);
			if (newNormalResult.IsSuccess) {
				macroService.PollPattern(patterns.mailbox.receiveAll, { DoClick: true, PredicatePattern: patterns.mailbox.tapTheScreen });
				macroService.PollPattern(patterns.mailbox.tapTheScreen, { DoClick: true, PredicatePattern: patterns.titles.mailbox });
			}

			logger.info('claimMail: claim operator');
			macroService.PollPattern(patterns.mailbox.operator, { DoClick: true, PredicatePattern: patterns.mailbox.operator.selected });
			const newOperatorResult = macroService.FindPattern(patterns.mailbox.new);
			if (newOperatorResult.IsSuccess) {
				macroService.PollPattern(patterns.mailbox.receiveAll, { DoClick: true, PredicatePattern: patterns.mailbox.tapTheScreen });
				macroService.PollPattern(patterns.mailbox.tapTheScreen, { DoClick: true, PredicatePattern: patterns.titles.mailbox });
			}

			logger.info('claimMail: claim message');
			macroService.PollPattern(patterns.mailbox.message, { DoClick: true, PredicatePattern: patterns.mailbox.message.selected });
			const newOperatorResult2 = macroService.FindPattern(patterns.mailbox.new);
			if (newOperatorResult2.IsSuccess) {
				macroService.PollPattern(patterns.mailbox.new, { DoClick: true, PredicatePattern: patterns.mailbox.message.receive });
				macroService.PollPattern(patterns.mailbox.message.receive, { DoClick: true, PredicatePattern: patterns.mailbox.tapTheScreen });
				macroService.PollPattern(patterns.mailbox.tapTheScreen, { DoClick: true, PredicatePattern: patterns.titles.mailbox });
			}
			
			logger.info('claimMail: done');
			if (macroService.IsRunning) {
				daily.claimMail.done.IsChecked = true;
			}
			return;
	}

	sleep(1_000);
}