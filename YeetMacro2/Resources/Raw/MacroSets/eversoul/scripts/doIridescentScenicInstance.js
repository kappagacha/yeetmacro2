// @position=11
// do operation eden alliance
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.titles.iridescentScenicInstance];
const daily = dailyManager.GetCurrentDaily();

if (daily.doIridescentScenicInstance.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('doIridescentScenicInstance: click adventure');
			macroService.ClickPattern(patterns.lobby.adventure);
			break;
		case 'titles.adventure':
			logger.info('doIridescentScenicInstance: click iridescent scenic insttance');
			macroService.PollPattern(patterns.adventure.tabs.specialMissions, { DoClick: true, PredicatePattern: patterns.iridescentScenicInstance });
			macroService.PollPattern(patterns.iridescentScenicInstance, { DoClick: true, PredicatePattern: patterns.titles.iridescentScenicInstance });
			break;
		case 'titles.iridescentScenicInstance':
			const zeroMaxSubjugationLevelResult = macroService.FindPattern(patterns.iridescentScenicInstance.zeroMaxSubjugationLevel);
			if (zeroMaxSubjugationLevelResult.IsSuccess) {
				logger.info('doIridescentScenicInstance: do unicat subjugation');

				const unicatPatterns = ['tofu', 'cheese', 'tabby', 'choco'].map(u => patterns.iridescentScenicInstance.unicats[u]);
				const unicatResult = macroService.PollPattern(unicatPatterns);
				const unicat = unicatResult.Path?.split('.').pop();
				logger.debug(`unicat: ${unicat}`);

				const targetLevel = settings.doIridescentScenicInstance.targetLevels[unicat].Value;
				if (!targetLevel) throw new Error(`Could not resolve target level for unicat: ${unicat}`);

				macroService.PollPattern(patterns.iridescentScenicInstance.subjugation, { DoClick: true, PredicatePattern: patterns.iridescentScenicInstance.levelSettings });
				setUnicatLevel(targetLevel);
				macroService.PollPattern(patterns.iridescentScenicInstance.subjugation.challenge, { DoClick: true, PredicatePattern: patterns.iridescentScenicInstance.subjugation.battle });
				macroService.PollPattern(patterns.iridescentScenicInstance.subjugation.battle, { DoClick: true, PredicatePattern: patterns.battle.start });
				macroService.PollPattern(patterns.battle.start, { DoClick: true, PredicatePattern: patterns.battle.exit });
				macroService.PollPattern(patterns.battle.exit, { DoClick: true, PredicatePattern: patterns.iridescentScenicInstance.subjugation.battle });

				let subjugationResult = macroService.FindPattern(patterns.iridescentScenicInstance.subjugation);
				while (macroService.IsRunning && !subjugationResult.IsSuccess) {
					macroService.ClickPattern(patterns.general.back);
					sleep(1_000);
					subjugationResult = macroService.FindPattern(patterns.iridescentScenicInstance.subjugation);
					sleep(1_000);
				}
			}

			const expeditionDispatchResult = macroService.PollPattern(patterns.iridescentScenicInstance.expeditionDispatch, { DoClick: true, PredicatePattern: [patterns.iridescentScenicInstance.expeditionDispatch.start, patterns.iridescentScenicInstance.expeditionDispatch.noSoulsSelected] });
			if (expeditionDispatchResult.PredicatePath === 'iridescentScenicInstance.expeditionDispatch.noSoulsSelected') {
				logger.info('doIridescentScenicInstance: select souls to dispatch');
				macroService.PollPattern(patterns.iridescentScenicInstance.expeditionDispatch.noSoulsSelected, { DoClick: true, PredicatePattern: patterns.iridescentScenicInstance.expeditionDispatch.cancel });

				const pctPatterns = [patterns.iridescentScenicInstance.expeditionDispatch.pct30, patterns.iridescentScenicInstance.expeditionDispatch.pct20, patterns.iridescentScenicInstance.expeditionDispatch.pct10];
				for (let pctPattern of pctPatterns) {
					if (!macroService.IsRunning) break;
					const pctResult = macroService.FindPattern(pctPattern, { Limit: 5 })
					for (let p of pctResult.Points) {
						if (!macroService.IsRunning || macroService.FindPattern(patterns.iridescentScenicInstance.expeditionDispatch.fiveSoulsSelected).IsSuccess)
							break;
						const checkPattern = macroService.ClonePattern(patterns.iridescentScenicInstance.expeditionDispatch.soulSelected, { CenterX: p.X + 110, CenterY: p.Y, Padding: 15, OffsetCalcType: 'None', Path: `iridescentScenicInstance.expeditionDispatch.soulSelected_x${p.X}_${p.Y}` });
						macroService.PollPoint(p, { DoClick: true, PredicatePattern: checkPattern });
					}
				}

				macroService.PollPattern(patterns.iridescentScenicInstance.expeditionDispatch.confirm, { DoClick: true, PredicatePattern: patterns.iridescentScenicInstance.expeditionDispatch.start });
				macroService.PollPattern(patterns.iridescentScenicInstance.expeditionDispatch.max, { DoClick: true, PredicatePattern: patterns.iridescentScenicInstance.expeditionDispatch.zeroSupplies });
				macroService.PollPattern(patterns.iridescentScenicInstance.expeditionDispatch.start, { DoClick: true, ClickPattern: patterns.iridescentScenicInstance.expeditionDispatch.skip, PredicatePattern: patterns.iridescentScenicInstance.expeditionDispatch.confirm2 });
				macroService.PollPattern(patterns.iridescentScenicInstance.expeditionDispatch.confirm2, { DoClick: true, ClickPattern: patterns.iridescentScenicInstance.expeditionDispatch.skip, PredicatePattern: patterns.iridescentScenicInstance.expeditionDispatch.zeroSupplies });
			} else {
				macroService.PollPattern(patterns.iridescentScenicInstance.expeditionDispatch.max, { DoClick: true, PredicatePattern: patterns.iridescentScenicInstance.expeditionDispatch.zeroSupplies });
				macroService.PollPattern(patterns.iridescentScenicInstance.expeditionDispatch.start, { DoClick: true, ClickPattern: patterns.iridescentScenicInstance.expeditionDispatch.skip, PredicatePattern: patterns.iridescentScenicInstance.expeditionDispatch.confirm2 });
				macroService.PollPattern(patterns.iridescentScenicInstance.expeditionDispatch.confirm2, { DoClick: true, ClickPattern: patterns.iridescentScenicInstance.expeditionDispatch.skip, PredicatePattern: patterns.iridescentScenicInstance.expeditionDispatch.zeroSupplies });
			}

			macroService.PollPattern(patterns.iridescentScenicInstance.expeditionDispatch.exit, { DoClick: true, PredicatePattern: patterns.iridescentScenicInstance.expeditionDispatch });

			if (macroService.IsRunning) {
				daily.doIridescentScenicInstance.done.IsChecked = true;
			}
			return;
	}

	sleep(1_000);
}

function setUnicatLevel(targetLevel) {
	let currentLevel = getCurrentUnicatLevel();
	while (macroService.IsRunning && currentLevel !== targetLevel) {
		const diff = targetLevel - currentLevel;
		if (diff > 100) {
			macroService.ClickPattern(patterns.iridescentScenicInstance.levelSettings.plus100);
		} else if (diff > 0) {
			macroService.ClickPattern(patterns.iridescentScenicInstance.levelSettings.plus25);
		} else if (diff <= -100) {
			macroService.ClickPattern(patterns.iridescentScenicInstance.levelSettings.minus100);
		} else { // less than 0 but greater than -100
			macroService.ClickPattern(patterns.iridescentScenicInstance.levelSettings.minus25);
		}

		sleep(500);
		currentLevel = getCurrentUnicatLevel();
		logger.debug(`currentLevel: ${currentLevel}, targetLevel: ${targetLevel}`);
	}
}

function getCurrentUnicatLevel() {
	// OCR is detecting the 6 as a 5
	const lvl600Result = macroService.FindPattern(patterns.iridescentScenicInstance.levelSettings.currentLevel600);
	if (lvl600Result.IsSuccess) return 600;

	return Number(macroService.FindText(patterns.iridescentScenicInstance.levelSettings.currentLevel)?.slice(0, 3));
}