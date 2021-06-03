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
            docker rm silksplugins-discordbot
            docker run -d -v /var/lib/docker/volumes/silksplugins-discordbot/_data:/storage --name silksplugins-discordbot silksplugins-discordbot:latest
        '''
    }
}