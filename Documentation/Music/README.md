# Trilha sonora

## Faixas
- `Assets/Audio/Music/menu_masky.mp3`: cena MainMenu.
- `Assets/Audio/Music/exploracao_masky.mp3`: Beach, House, Island, Cave, LighthouseExterior e Lighthouse.

O GameAudio existente mantém uma única saída persistente de música. A faixa de exploração não reinicia ao trocar entre essas áreas. As faixas tocam em loop; trocas usam fade de 0,65 segundo. O painel de vídeo do menu e as cenas de encerramento não recebem trilha de fundo contínua. Os vídeos preservam seu áudio original.

Volume = ganho da biblioteca (0,35) × preferência Música × fade. O controle Geral continua aplicado pelo AudioListener. Durante pausa o volume da música é reduzido a 45% do nível normal. Efeitos sonoros mantêm seu canal separado.

## Arquivos
- Modificado `Assets/Scripts/GameAudio.cs`: seleção de faixa, continuidade, fade e volume.
- Criado `Assets/Scripts/GameMusicLibrary.cs`: referências às músicas e ganho configurável.
- Criado `Assets/Resources/GameMusic.asset`: aponta para os MP3 originais, sem duplicá-los.
- Metadados dos MP3: importação por streaming.
- Criados `Tests/UIAuthoring/AuthorGameMusic.cs` e `Tests/GameplayValidation/MusicValidation.cs`: configuração e validação na cópia isolada.

## Configuração manual
Nenhuma. Para ajustar o volume base, abra `Assets/Resources/GameMusic.asset` no Inspector e altere Gain. O jogador também pode usar Configurações → Áudio → Música.

## Verificação
60 verificações aprovadas. Resultados em `results.txt`. Os testes cobrem faixas, loop, volume, transições e persistência, além de regressão do lampião, Sombra e encerramento. A escuta final e o ajuste de gosto pessoal ainda podem ser feitos jogando; não foi realizado corte ou remasterização dos arquivos fornecidos.

A versão Windows e a publicação não foram atualizadas.
