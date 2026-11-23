# Transições de menu e cenas

## Fluxo e durações

- Splash → nickname: preserva o clique com partículas e fade da Splash; MainMenu revela o nickname com fade-in de 0,47 s.
- Continuar: valida/salva o nickname e escurece em 0,47 s antes de trocar os painéis.
- Introdução: prepara o VideoPlayer sob a cobertura, revela o vídeo com fade-in, espera seu término e faz fade-out antes de revelar o menu. Cada fade dura 0,47 s. Clip e RenderTexture existentes permanecem atribuídos.
- Jogar → Cave: fade-out de 0,18 s, carregamento assíncrono sob cobertura e fade-in de 0,18 s.
- Portas Cave ↔ Island: mesma transição rápida. A duração total inclui o tempo necessário para carregar a cena; não existe espera artificial extra.

A cor azul quase preta e a curva SmoothStep seguem o efeito da Splash. Todas as animações usam tempo real e funcionam mesmo com Time.timeScale em zero. As transições não pausam nem reiniciam a missão existente nas portas.

## Arquivos

- Criados: Assets/Scripts/SceneFade.cs e SceneFade.cs.meta. A cobertura é um Canvas temporário que sobrevive apenas à troca de cena e se destrói ao terminar. Não guarda referências à UI das cenas.
- Modificado MainMenuController.cs: preserva os fades locais encontrados no projeto e usa SceneFade ao clicar Jogar. Mantém o menu visível durante o escurecimento e bloqueia novos comandos.
- Modificado PlayerClickMovement.cs: portas usam SceneFade; IsMovementEnabled também bloqueia ações enquanto a transição está ativa. Movimento, colisão, destinos e distâncias das portas foram preservados.
- Atualizado Tests/GameplayValidation/GameplayValidationRunner.cs: verifica os fades do menu, cobertura da cena de destino, carregamentos repetidos, transição pela porta real e restauração dos controles, além da regressão de gameplay.
- Nenhum arquivo .unity, prefab, asset de vídeo, script de mineração ou gerenciador foi alterado nesta etapa.

## Unity Editor

Nenhuma atribuição manual ou novo asset é necessário. Os overlays são criados durante Play Mode. A duração do menu pode ser ajustada em MainMenuController → Fade Duration; a duração rápida está centralizada em SceneFade.QuickDuration.

Conferir visualmente: Splash → nickname, Continuar → vídeo, término do vídeo → menu, Jogar → Cave e portas Cave ↔ Island. Testar o vídeo real com áudio e observar ausência de flashes nas diferentes resoluções. O teste batch sem GPU simula o callback do vídeo; não valida sua decodificação ou a aparência final.

O resultado da execução automatizada desta revisão está em TransitionsValidationResults.txt.

## Resultado

Compilação no Unity sem erros e 72 verificações aprovadas (ALL PASSED), incluindo regressão de configurações, missão e mineração. As quatro cenas passaram na checagem de IDs e referências locais. O aviso de índice de busca do Editor foi registrado separadamente; nenhuma exceção dos scripts do jogo foi ignorada.
