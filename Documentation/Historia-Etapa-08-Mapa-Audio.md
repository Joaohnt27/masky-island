# Etapa 08 — Mapa e efeitos sonoros

## Mecânicas
- M ou botão MAPA abre o mapa dos cinco cenários; M/Esc fecha.
- Local atual, objetivo da história, caminhos e prévias dos cenários.
- Viagem rápida somente para locais visitados nesta partida. O local atual não é destino de viagem.
- Cristais, encaixes e lembranças permanecem ao viajar; uma nova partida limpa os destinos visitados.
- Movimento bloqueado durante o mapa; abertura impedida durante puzzles, diálogo, mineração, inventário, pausa e transições.
- Onze efeitos originais sintetizados: clique, abrir/fechar, picareta, cristal, sucesso, erro, confirmação, viagem, farol e passos.
- Áudio persistente com canais separados para interface e ações, integrado ao volume Geral/Efeitos existente. A pausa interrompe o canal de ações.

## Arquivos criados
- Assets/Scripts/IslandMapUI.cs: estado e interação do mapa.
- Assets/Scripts/IslandMapGraphic.cs: desenho da ilha e caminhos no Canvas.
- Assets/Scripts/GameAudio.cs: reprodução persistente dos efeitos.
- Assets/Scripts/GameplayAudioFeedback.cs: passos, golpes e recompensas.
- Assets/Scripts/ButtonSound.cs: feedback nos botões existentes.
- Assets/Resources/CartoonSfx/*.wav: 11 efeitos, com respectivos .meta.
- Tests/AudioAuthoring/CreateCartoonSfx.py: geração reproduzível dos sons originais.
- Tests/UIAuthoring/AuthorMapAudio.cs: configuração incremental das cenas na cópia de autoria.
- Tests/GameplayValidation/MapAudioValidation.cs: validação adicional do mapa e áudio.
- Documentation/MapAudio/CartoonSfxPreview.wav: prévia sequencial dos efeitos.

## Arquivos modificados
- GameManager.cs: registra visitas e expõe destino do objetivo; mantém a fonte única de progresso.
- StoryLocation.cs: expõe objetivo/foco para o mapa.
- PauseController.cs: Esc fecha o mapa antes de pausar.
- SettingsAudioChannel.cs: permite selecionar categoria dos canais criados em runtime.
- SceneFade.cs: efeito de viagem nas transições de exploração.
- CrystalInventoryUI.cs: sons de abertura/fechamento.
- MiningSpot.cs e MiningUI.cs: feedback de tentativa sem cristal e encaixe incorreto.
- SymbolSequencePuzzle.cs e LighthouseInteraction.cs: confirmação/erro.
- Beach, House, Island, Cave e Lighthouse: mapa configurado, botão e indicação de tecla M, feedback sonoro.
- MainMenu e Credits: sons de botões. Ending: AudioListener para continuidade do efeito final.

## Configuração manual no Unity
Nenhum arraste de referências necessário: cenas já configuradas. Abra MainMenu e inicie uma nova partida. Ajuste Configurações → Áudio → Efeitos para experimentar o volume.

## Assets e limites
Nenhum asset obrigatório faltante para esta etapa. Efeitos sintetizados originais, sem gravações externas ou dependências adicionais. O mapa é esquemático e usa prévias das ilustrações existentes. Visitas/progresso persistem durante a partida, sem salvamento em disco. A apreciação do timbre e do volume final precisa de audição no computador do jogador.

## Testes
- Unity 6: compilação concluída sem erros.
- 35 verificações de mapa/áudio aprovadas: MapAudio/MapAudioResults.txt.
- 38 verificações de regressão da história aprovadas: MapAudio/StoryRegressionResults.txt. Incluem mineração, inventário, puzzles, QTE, cutscene, créditos e reinício.
- Prévia visual do mapa revisada em 1280×720: MapAudio/map-first-visit.png e MapAudio/map-objective.png.
- Os 11 WAVs foram verificados: mono PCM 16 bits, 44,1 kHz, pico máximo 0,55, sem saturação digital.
- Foi registrada uma exceção conhecida do índice de busca do Editor (UnityEditor.Search.SearchDatabase), isolada das verificações de gameplay. Nenhuma exceção de gameplay foi aceita pelo teste.
- Correção durante validação: a cópia isolada inicialmente recebeu os WAVs fora da subpasta CartoonSfx; corrigido e testes reexecutados com sucesso.
- Avaliação auditiva subjetiva e outras proporções de tela permanecem para revisão manual. Não foram adicionadas música ambiente ou dublagem nesta etapa.
