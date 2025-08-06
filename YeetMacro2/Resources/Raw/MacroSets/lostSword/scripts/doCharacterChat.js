// @position=5
const loopPatterns = [patterns.lobby, patterns.character.title, patterns.character.chat.title, patterns.character.chat.done, patterns.character.chat.chatAvailable, patterns.character.chat.options, patterns.character.chat.chatConsumed];
const daily = dailyManager.GetCurrentDaily();
if (daily.doCharacterChat.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.character.chat.next });
	switch (loopResult.Path) {
		case 'lobby':
			logger.info('doCharacterChat: click character');
			macroService.ClickPattern(patterns.character);
			break;
		case 'character.title':
			logger.info('doCharacterChat: click character chat');
			macroService.ClickPattern(patterns.character.chat);
			break;
		case 'character.chat.title':
			logger.info('doCharacterChat: click a character');
			macroService.ClickPattern(patterns.character.chat.chatCount, { ClickOffset: { Y: 100 }});
			break;
		case 'character.chat.done':
			logger.info('doCharacterChat: done');
			if (macroService.IsRunning)
				daily.doCharacterChat.done.IsChecked = true;
			return;
		case 'character.chat.chatAvailable':
			logger.info('doCharacterChat: start chat');
			macroService.PollPattern(patterns.character.chat.chat, { DoClick: true, PredicatePattern: patterns.character.chat.log });
			macroService.PollPattern(patterns.character.chat.next, { DoClick: true, PredicatePattern: patterns.character.chat.options });
			break;
		case 'character.chat.options':
			logger.info('doCharacterChat: choose chat option');
			const optionsResult = macroService.FindPattern(patterns.character.chat.options, { Limit: 3 });
			const optionNumber = Math.floor(Math.random() * optionsResult.Points.length);
			const optionPoint = optionsResult.Points[optionNumber];
			macroService.DoClick(optionPoint);
			break;
		case 'character.chat.chatConsumed':
			logger.info('doCharacterChat: chatConsumed so choose new chat');
			const selectedCharacterResult = macroService.FindPattern(patterns.character.chat.selectedCharacter);
			macroService.DoClick(selectedCharacterResult.Point.Offset(100, 150));
			break;

	}
	sleep(1_000);
}