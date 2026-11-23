# Vídeo final após o farol

O sucesso no quick time event do farol mantém a vitória e carrega a cena Ending. Ela agora reproduz `Assets/Videos/final_gameplay.mp4`, respeitando a proporção do vídeo e o volume configurado. Ao terminar, segue para Credits. Clique em Pular ou Esc também segue para os créditos, sem duplicar a transição.

## Arquivos
- Modificado: `Assets/Scripts/EndingCutscene.cs`, reprodução e encerramento do vídeo.
- Modificada: `Assets/Scenes/Ending.unity`, referência ao arquivo fornecido pelo usuário.
- Novo: `Tests/GameplayValidation/FinalGameplayValidation.cs`.

O QTE existente não foi alterado. Falha no QTE continua permitindo nova tentativa sem reproduzir o vídeo. Se o vídeo estiver ausente ou não puder ser preparado/reproduzido, o encerramento ilustrado anterior permanece como alternativa para não bloquear o jogo. A preparação tem limite de tempo.

Nenhuma configuração manual ou novo asset necessário: o MP4 já fornecido foi conectado à cena.

## Validação
7 verificações aprovadas no Unity 6 isolado: falha do QTE sem vídeo, vitória com transição, decodificação do MP4 real, substituição das legendas anteriores, botão clicável, créditos automáticos e pular com clique repetido. Captura revisada visualmente e compilação sem erros. O Unity ajustou timestamps do arquivo H.264 durante a reprodução; o vídeo terminou normalmente. A exceção conhecida do índice de busca do Editor foi registrada separadamente dos testes de gameplay. Resultados em Documentation/FinalGameplay.

