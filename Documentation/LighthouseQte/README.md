# A última luz — desafio do farol

O QTE foi adaptado para o estilo da bancada de fabricação da chave. Depois de clicar em PREPARAR MECANISMO, confirme com o botão AJUSTAR ou com Espaço quando o indicador estiver dentro da faixa dourada.

## Etapas
1. Alinhar as lentes.
2. Regular as engrenagens.
3. Acender a luz.

O indicador fica um pouco mais rápido e a faixa um pouco menor a cada etapa. Cada ajuste tem oito segundos. Errar permite tentar novamente; se o tempo acabar, somente a etapa atual recomeça. Acertos anteriores permanecem durante a tentativa. Cliques repetidos têm um intervalo de proteção. Voltar/Esc cancela e devolve o movimento; reabrir começa pelos três ajustes novamente. Pausa externa congela o indicador, o prazo e a confirmação.

A conclusão acende o farol, mostra a confirmação e leva à cena Ending com final_gameplay. Continua sendo necessário concluir a fabricação e destrancar o farol.

## UI
Pergaminho reutilizado do jogo, fonte DynaPuff, três etapas com indicação de conclusão, detalhe ilustrado do mecanismo, barra de precisão verde/dourada, indicador com contorno e barra menor de tempo. Nenhuma nova imagem externa foi necessária.

## Arquivos
Modificados:
- Assets/Scripts/LighthouseInteraction.cs: desafio de precisão no controlador existente, tentativas, pausa, feedback e final.
- Assets/Scripts/PauseController.cs: Esc fecha o painel do farol como já faz na bancada.
- Assets/Scenes/Lighthouse.unity: nova apresentação e referências configuradas.
- Tests/GameplayValidation/LighthouseExteriorValidation.cs: confirmações adaptadas ao novo desafio (arquivo atualizado, não reexecutado nesta etapa).

Criados:
- Tests/UIAuthoring/AuthorLighthouseQte.cs.
- Tests/GameplayValidation/LighthouseQteValidation.cs.
- Relatório, resultados e capturas nesta pasta.

## Configuração e validação
Nenhuma configuração manual no Inspector é necessária. Sem assets pendentes.

Unity 6000.6.0f1 em Builds/PV: 22 verificações incluindo abertura, movimento bloqueado, confirmação incorreta, pausa, cancelamento, tempo esgotado, manutenção de etapas concluídas, clique, Espaço, proteção contra ações repetidas, vitória, transição para Ending e reprodução de frames reais do vídeo final. Capturas revisadas em 1280×720. O teste prepara o progresso anterior para exercitar o QTE; não repete toda a aventura desde o menu.

Backup da cena e do controlador anterior em Builds/LighthouseQteBackup. Os testes registram separadamente a exceção conhecida do índice de busca do Editor.
