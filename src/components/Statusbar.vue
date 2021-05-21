<template>
  <div id='statusbar'>
    <p id='status-bar' class='form-control'> Last updated: {{this.timeOfReq}} </p>
  </div>
</template>

<script>
import axios from "axios";

export default {
  name: "Statusbar",
  async mounted () {
    await this.getData()
  },
  data() {
    return {
      timeOfReq: String
    }
  },
  methods: {
    async getData () {
      const timeOfRequest = await axios.get('https://localhost:5001/timeOfRequest')
      this.timeOfReq = timeOfRequest.data.toString()
    }
  }
}

</script>

<style scoped>
#status-bar {
  text-align: center;
  width: 360px;
  position: absolute;
  bottom: 0;
  left: 550px;
  margin-bottom: 20px;
}
</style>
