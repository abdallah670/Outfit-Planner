const fs = require('fs'); const content = fs.readFileSync('src/outfit-planner-ui/src/app/core/state/user/user.effects.ts', 'utf8'); console.log(content.substring(0, 500));  
