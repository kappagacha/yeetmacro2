// @position=15
// Shops weekly survey hub items
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.titles.surveyHub];
const weekly = weeklyManager.GetCurrentWeekly();
const resolution = macroService.GetCurrentResolution();

if (weekly.doWeeklySurveyHubShop.done.IsChecked) {
	return "Script already completed. Uncheck done to override weekly flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('doWeeklySurveyHubShop: click adventure tab');
			macroService.ClickPattern(patterns.tabs.adventure);
			sleep(500);
			break;
		case 'titles.adventure':
			logger.info('doWeeklySurveyHubShop: go to survey hub');
			macroService.PollPattern(patterns.adventure.adventure, { DoClick: true, PredicatePattern: patterns.adventure.surveyHub });
			sleep(500);
			macroService.PollPattern(patterns.adventure.surveyHub, { DoClick: true, PredicatePattern: patterns.titles.surveyHub });
			sleep(500);
			break;
		case 'titles.surveyHub':
			logger.info('doWeeklySurveyHubShop: buy weekly items');
			const startX = resolution.Width - 100;
			const endX = startX - 700;
			for (let i = 0; i < 3; i++) {
				macroService.DoSwipe({ X: startX, Y: 500 }, { X: endX, Y: 500 });
				sleep(1_000);
			}

			macroService.PollPattern(patterns.surveyHub.surveyHubItems.season1, { DoClick: true, PredicatePattern: patterns.surveyHub.surveyHubItems.season1.enabled });
			const season1surveyHubItems = ['intermediateSkillManual', '10pctLegendaryAbrasive'];
			//const season1surveyHubItems = ['epicReforgeCatalyst', 'superiorQualityPresentChest', 'basicSkillManual', 'intermediateSkillManual', '10pctLegendaryAbrasive'];
			for (const surveyHubItem of season1surveyHubItems) {
				purchaseSurveyHubItem(surveyHubItem);
			}

			macroService.PollPattern(patterns.surveyHub.surveyHubItems.season2, { DoClick: true, PredicatePattern: patterns.surveyHub.surveyHubItems.season2.enabled });
			macroService.DoSwipe({ X: startX, Y: 500 }, { X: endX, Y: 500 });

			sleep(1_000);
			const season2surveyHubItems = ['legendaryReforgeCatalyst', 'professionalSkillManual', 'refinedGlunite', 'epicQualityPresentChest'];
			for (const surveyHubItem of season2surveyHubItems) {
				purchaseSurveyHubItem(surveyHubItem);
			}

			if (macroService.IsRunning) {
				weekly.doWeeklySurveyHubShop.done.IsChecked = true;
			}

			return;
	}
	sleep(1_000);
}

function purchaseSurveyHubItem(surveyHubItem) {
	if (settings.doWeeklySurveyHubShop.surveyHubItems[surveyHubItem].Value && !weekly.doWeeklySurveyHubShop.surveyHubItems[surveyHubItem].IsChecked) {
		logger.info(`doWeeklySurveyHubShop: purchase surveyHub item ${surveyHubItem}`);
		const surveyHubItemPattern = macroService.ClonePattern(patterns.surveyHub.surveyHubItems[surveyHubItem], {
			X: 150,
			Y: 230,
			Width: resolution.Width - 250,
			Height: 800,
			Path: `patterns.shop.resource.surveyHub.${surveyHubItem}`,
			OffsetCalcType: 'DockLeft'
		});
		const surveyHubItemResult = macroService.PollPattern(surveyHubItemPattern);
		const surveyHubItemPurchasePattern = macroService.ClonePattern(patterns.surveyHub.purchase, {
			X: surveyHubItemResult.Point.X - 50,
			Y: surveyHubItemResult.Point.Y + 200,
			Width: 350,
			Height: 140,
			Path: `patterns.shop.resource.surveyHub.${surveyHubItem}.purchase`,
			OffsetCalcType: 'None'
		});

		macroService.PollPattern(surveyHubItemPurchasePattern, { DoClick: true, PredicatePattern: patterns.shop.resource.ok });
		const maxSliderResult = macroService.FindPattern(patterns.shop.resource.maxSlider);
		if (maxSliderResult.IsSuccess) {
			macroService.PollPattern(patterns.shop.resource.maxSlider, { DoClick: true, InversePredicatePattern: patterns.shop.resource.maxSlider });
		}
		macroService.PollPattern(patterns.shop.resource.ok, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
		macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.surveyHub });
		if (macroService.IsRunning) {
			weekly.doWeeklySurveyHubShop.surveyHubItems[surveyHubItem].IsChecked = true;
		}
	}
}