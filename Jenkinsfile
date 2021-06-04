node {
    stage('Clone repository') {
        git branch: 'main', credentialsId: 'github-app-SilKsPlugins', url: 'https://github.com/SilKsPlugins/SilKsPlugins.DiscordBot'
    }
    
    stage('Build image') {
        app = docker.build("silksplugins-discordbot")
    }
    
    stage('Push image') {
        docker.withRegistry('http://127.0.0.1:6000') {
            app.push("0.1.${env.BUILD_NUMBER}")
            app.push('latest')
        }
    }
    
    stage('Deploy container') {
        sh '''
            docker ps -q --filter "name=silksplugins-discordbot" | grep -q . && docker stop silksplugins-discordbot
            docker ps -a -q --filter "name=silksplugins-discordbot" | grep -q . && docker rm -fv silksplugins-discordbot
            docker run -d -v silksplugins-discordbot:/storage --name silksplugins-discordbot silksplugins-discordbot:latest
        '''
    }
}